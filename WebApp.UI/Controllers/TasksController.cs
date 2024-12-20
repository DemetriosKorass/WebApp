using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using WebApp.DAL;
using WebApp.UI.Services.ExtensionMethods;
using WebApp.UI.ViewModels;
using Task = WebApp.DAL.Entities.Task;

namespace WebApp.UI.Controllers
{
    [Authorize]
    public class TasksController(AppDbContext context, IServiceProvider serviceProvider) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var tasks = await context.Tasks
                .Include(t => t.Users)
                .ToListAsync();
            return View(tasks);
        }

        public IActionResult Create()
        {
            return View(new Task());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        public IActionResult BulkCreate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkCreate(string taskNames, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(taskNames))
            {
                ModelState.AddModelError("", "No task names provided.");
                return View();
            }

            // Split the input string into lines, removing empty entries and trimming whitespace
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

            // Asynchronously fetch existing task names
            var existingNames = await context.Tasks
                .Select(t => t.Name)
                .ToListAsync(cancellationToken);
            var existingNameSet = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);

            // Remove task names that already exist
            uniqueTaskNames.RemoveWhere(name => existingNameSet.Contains(name));

            if (uniqueTaskNames.Count == 0)
            {
                ModelState.AddModelError("", "All provided task names already exist.");
                return View();
            }

            // Prepare new Task entities
            var newTasks = uniqueTaskNames.Select(name => new Task { Name = name }).ToList();

            // Add new tasks to the context
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


        [HttpPost]
        [ValidateAntiForgeryToken]
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

        public async Task<IActionResult> Edit(int id)
        {
            var task = await context.Tasks
                .Include(t => t.Users)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();

            ViewBag.AllUsers = await context.Users.ToListAsync();

            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

        public async Task<IActionResult> Delete(int id)
        {
            var task = await context.Tasks.FindAsync(id);
            if (task == null) return NotFound();
            return View(task);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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
