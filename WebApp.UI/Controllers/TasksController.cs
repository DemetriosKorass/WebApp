using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WebApp.DAL;
using WebApp.UI.Services.ExtensionMethods;
using WebApp.UI.ViewModels;
using Task = WebApp.DAL.Entities.Task;

namespace WebApp.UI.Controllers
{
    /// <summary>
    /// Manages operations related to tasks, including listing, creation, bulk creation, user assignments, editing, and deletion.
    /// </summary>
    [Authorize]
    public class TasksController(AppDbContext context, IServiceProvider serviceProvider) : Controller
    {

        /// <summary>
        /// Displays a list of all tasks, including their associated users.
        /// </summary>
        /// <returns>A view containing the list of tasks.</returns>
        [HttpGet("[Controller]")]
        [SwaggerOperation(Summary = "List Tasks", Description = "Displays a list of all tasks with their associated users.")]
        public async Task<IActionResult> Index()
        {
            var tasks = await context.Tasks
                .Include(t => t.Users)
                .ToListAsync();
            return View(tasks);
        }

        /// <summary>
        /// Displays the form to create a new task.
        /// </summary>
        /// <returns>A view containing the task creation form.</returns>
        [HttpGet("[Controller]/[Action]")]
        [SwaggerOperation(Summary = "Create Task Form", Description = "Displays the form for creating a new task.")]
        public IActionResult Create()
        {
            return View(new Task());
        }

        /// <summary>
        /// Processes the creation of a new task. Validates the task name and ensures uniqueness before saving.
        /// </summary>
        /// <param name="task">The task entity to be created.</param>
        /// <returns>Redirects to the Index view upon successful creation or redisplays the form with errors.</returns>
        [HttpPost("[Controller]/[Action]")]
        [ValidateAntiForgeryToken]
        [SwaggerOperation(Summary = "Create Task", Description = "Processes the creation of a new task.")]
        public async Task<IActionResult> Create(Task task)
        {
            if (string.IsNullOrWhiteSpace(task.Name))
            {
                ModelState.AddModelError("Name", "Task name is required.");
            }
            if (!ModelState.IsValid)
            {
                return View(task);
            }

            var existingNames = await context.Tasks.Select(t => t.Name).ToListAsync();
            var nameSet = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);
            if (nameSet.Contains(task.Name))
            {
                ModelState.AddModelError("Name", "Task with this name already exists.");
                return View(task);
            }

            context.Tasks.Add(task);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Displays the form to bulk create tasks by entering multiple task names.
        /// </summary>
        /// <returns>A view containing the bulk task creation form.</returns>
        [HttpGet("[Controller]/[Action]")]
        [SwaggerOperation(Summary = "Bulk Create Task Form", Description = "Displays the form for bulk creating tasks.")]
        public IActionResult BulkCreate()
        {
            return View();
        }

        /// <summary>
        /// Processes the bulk creation of tasks. Splits input task names, validates uniqueness, and creates tasks in parallel.
        /// </summary>
        /// <param name="taskNames">A newline-separated string of task names to be created.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>Redirects to the Index view upon successful creation or redisplays the form with errors.</returns>
        [HttpPost("[Controller]/[Action]")]
        [ValidateAntiForgeryToken]
        [SwaggerOperation(Summary = "Bulk Create Tasks", Description = "Processes the bulk creation of tasks.")]
        public async Task<IActionResult> BulkCreate(string taskNames, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(taskNames))
            {
                ModelState.AddModelError("", "No task names provided.");
                return View();
            }

            var taskNameList = taskNames
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .Select(name => name.Trim())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList();

            if (taskNameList.Count == 0)
            {
                ModelState.AddModelError("", "No valid task names provided.");
                return View();
            }

            var uniqueTaskNames = new HashSet<string>(taskNameList, StringComparer.OrdinalIgnoreCase);

            var existingNames = await context.Tasks
                .Select(t => t.Name)
                .ToListAsync(cancellationToken);
            var existingNameSet = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);

            uniqueTaskNames.RemoveWhere(name => existingNameSet.Contains(name));

            if (uniqueTaskNames.Count == 0)
            {
                ModelState.AddModelError("", "All provided task names already exist.");
                return View();
            }

            var newTasks = uniqueTaskNames.Select(name => new Task { Name = name }).ToList();

            var parallelCount = Environment.ProcessorCount;
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = parallelCount,
                CancellationToken = cancellationToken
            };
            try
            {
                int batchSize = newTasks.Count / parallelCount + 1;
                Parallel.ForEach(newTasks.Split(batchSize), parallelOptions, async task =>
                {
                    using var scope = serviceProvider.CreateScope();

                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    dbContext.Tasks.AddRange(task);

                    await dbContext.SaveChangesAsync(cancellationToken);
                });
                ViewBag.Message = $"{newTasks.Count} tasks were successfully created.";
            }
            catch (OperationCanceledException)
            {
                ModelState.AddModelError("", "Task creation was cancelled.");
                return View();
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Displays the form to assign users to tasks in bulk.
        /// </summary>
        /// <returns>A view containing the bulk user assignment form.</returns>
        [HttpGet("[Controller]/[Action]")]
        [SwaggerOperation(Summary = "Bulk Assign Users to Tasks", Description = "Displays the form for bulk assigning users to tasks.")]
        public async Task<IActionResult> AssignUsersBulk()
        {
            var tasks = await context.Tasks.Include(t => t.Users).ToListAsync();
            var users = await context.Users.ToListAsync();
            var viewModel = new AssignUsersBulkViewModel
            {
                Tasks = tasks,
                Users = users,
                TaskAssignments = tasks.Select(t => new TaskAssignment
                {
                    TaskId = t.Id,
                    SelectedUserIds = t.Users.Select(u => u.Id).ToList()
                }).ToList()
            };
            return View(viewModel);
        }

        /// <summary>
        /// Processes the bulk assignment of users to tasks based on the provided view model.
        /// </summary>
        /// <param name="viewModel">The view model containing task assignments.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>Redirects to the Index view upon successful assignment or redisplays the form with errors.</returns>
        [HttpPost("[Controller]/[Action]")]
        [ValidateAntiForgeryToken]
        [SwaggerOperation(Summary = "Bulk Assign Users to Tasks", Description = "Processes the bulk assignment of users to tasks.")]
        public async Task<IActionResult> AssignUsersBulk(AssignUsersBulkViewModel viewModel, CancellationToken cancellationToken)
        {
            if (viewModel.TaskAssignments == null || viewModel.TaskAssignments.Count == 0)
            {
                ModelState.AddModelError("", "No assignments provided.");
                viewModel.Tasks = await context.Tasks.Include(t => t.Users).ToListAsync(cancellationToken);
                viewModel.Users = await context.Users.ToListAsync(cancellationToken);
                return View(viewModel);
            }

            foreach (var assignment in viewModel.TaskAssignments)
            {
                var task = await context.Tasks
                    .Include(t => t.Users)
                    .FirstOrDefaultAsync(t => t.Id == assignment.TaskId, cancellationToken);

                if (task == null) continue;

                var selectedUsers = await context.Users
                    .Where(u => assignment.SelectedUserIds.Contains(u.Id))
                    .ToListAsync(cancellationToken);

                task.Users.Clear();
                task.Users.AddRange(selectedUsers);
            }

            try
            {
                await context.SaveChangesAsync(cancellationToken);
                TempData["Message"] = "Users have been assigned to tasks successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while assigning users: {ex.Message}");
            }

            viewModel.Tasks = await context.Tasks.Include(t => t.Users).ToListAsync(cancellationToken);
            viewModel.Users = await context.Users.ToListAsync(cancellationToken);
            return View(viewModel);
        }

        /// <summary>
        /// Displays the form to edit an existing task.
        /// </summary>
        /// <param name="id">The identifier of the task to edit.</param>
        /// <returns>A view containing the task edit form or a NotFound result if the task does not exist.</returns>
        [HttpGet("[Controller]/[Action]/{id}")]
        [SwaggerOperation(Summary = "Edit Task Form", Description = "Displays the form for editing an existing task.")]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await context.Tasks
                .Include(t => t.Users)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();

            ViewBag.AllUsers = await context.Users.ToListAsync();

            return View(task);
        }

        /// <summary>
        /// Processes the editing of an existing task, updating its name and assigned users.
        /// </summary>
        /// <param name="id">The identifier of the task being edited.</param>
        /// <param name="Name">The new name for the task.</param>
        /// <param name="SelectedUsers">An array of user identifiers to assign to the task.</param>
        /// <returns>Redirects to the Index view upon successful editing or redisplays the form with errors.</returns>
        [HttpPost("[Controller]/[Action]/{id}")]
        [ValidateAntiForgeryToken]
        [SwaggerOperation(Summary = "Edit Task", Description = "Processes the editing of an existing task.")]
        public async Task<IActionResult> Edit(int id, string Name, int[] SelectedUsers)
        {
            var task = await context.Tasks
                .Include(t => t.Users)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();

            if (string.IsNullOrWhiteSpace(Name))
            {
                ModelState.AddModelError("Name", "Task name is required.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.AllUsers = await context.Users.ToListAsync();
                task.Name = Name ?? string.Empty;
                return View(task);
            }

            task.Name = Name;
            task.Users.Clear();
            if (SelectedUsers.Length != 0)
            {
                var users = await context.Users
                    .Where(u => SelectedUsers.Contains(u.Id))
                    .ToListAsync();
                task.Users.AddRange(users);
            }

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Displays the confirmation page to delete a specific task.
        /// </summary>
        /// <param name="id">The identifier of the task to delete.</param>
        /// <returns>A view containing the delete confirmation or a NotFound result if the task does not exist.</returns>
        [HttpGet("[Controller]/[Action]/{id}")]
        [SwaggerOperation(Summary = "Delete Task Confirmation", Description = "Displays the confirmation page for deleting a task.")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await context.Tasks.FindAsync(id);
            if (task == null) return NotFound();
            return View(task);
        }

        /// <summary>
        /// Processes the deletion of a specific task after confirmation.
        /// </summary>
        /// <param name="id">The identifier of the task to delete.</param>
        /// <returns>Redirects to the Index view upon successful deletion or a NotFound result if the task does not exist.</returns>
        [HttpPost("[Controller]/[Action]/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [SwaggerOperation(Summary = "Delete Task", Description = "Processes the deletion of a specific task.")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            context.Tasks.Remove(task);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
