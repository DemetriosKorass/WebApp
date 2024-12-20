using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.DAL;
using WebApp.DAL.Entities;
using WebApp.UI.ViewModels;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApp.UI.Controllers
{
    /// <summary>
    /// Manages user-related operations including listing, creation, editing, and deletion of users.
    /// </summary>
    [Authorize]
    public class UsersController(AppDbContext context) : Controller
    {

        /// <summary>
        /// Displays a list of all users, including their associated roles and tasks.
        /// </summary>
        /// <returns>A view containing the list of users.</returns>
        [HttpGet]
        [SwaggerOperation(Summary = "List Users", Description = "Displays a list of all users with their roles and assigned tasks.")]
        public async Task<IActionResult> Index()
        {
            var users = await context.Users
                .Include(u => u.Role)
                .Include(u => u.Tasks)
                .ToListAsync();
            return View(users);
        }

        /// <summary>
        /// Displays the form to create a new user.
        /// </summary>
        /// <returns>A view containing the user creation form.</returns>
        [HttpGet("[Controller]/[Action]")]
        [SwaggerOperation(Summary = "Create User Form", Description = "Displays the form for creating a new user.")]
        public async Task<IActionResult> Create()
        {
            var roles = await context.Roles.ToListAsync();
            var tasks = await context.Tasks.ToListAsync();
            var viewModel = new UserCreateViewModel
            {
                Roles = roles,
                AllTasks = tasks
            };
            return View(viewModel);
        }

        /// <summary>
        /// Processes the creation of a new user. Validates input and ensures role and task assignments are valid.
        /// </summary>
        /// <param name="viewModel">The view model containing user creation data.</param>
        /// <returns>Redirects to the Index view upon successful creation or redisplays the form with errors.</returns>
        [HttpPost("[Controller]/[Action]")]
        [ValidateAntiForgeryToken]
        [SwaggerOperation(Summary = "Create User", Description = "Processes the creation of a new user.")]
        public async Task<IActionResult> Create(UserCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Roles = await context.Roles.ToListAsync();
                viewModel.AllTasks = await context.Tasks.ToListAsync();
                return View(viewModel);
            }

            if (!viewModel.SelectedRoleId.HasValue)
            {
                ModelState.AddModelError("SelectedRoleId", "Role is required.");
                viewModel.Roles = await context.Roles.ToListAsync();
                viewModel.AllTasks = await context.Tasks.ToListAsync();
                return View(viewModel);
            }

            var selectedRole = await context.Roles.FindAsync(viewModel.SelectedRoleId.Value);
            if (selectedRole == null)
            {
                ModelState.AddModelError("SelectedRoleId", "Selected role does not exist.");
                viewModel.Roles = await context.Roles.ToListAsync();
                viewModel.AllTasks = await context.Tasks.ToListAsync();
                return View(viewModel);
            }

            var user = new User
            {
                Name = viewModel.Name,
                Email = viewModel.Email,
                Role = selectedRole
            };

            if (viewModel.SelectedTasks.Length != 0)
            {
                var selectedTasks = await context.Tasks
                    .Where(t => viewModel.SelectedTasks.Contains(t.Id))
                    .ToListAsync();
                user.Tasks = selectedTasks;
            }

            context.Users.Add(user);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Displays the form to edit an existing user.
        /// </summary>
        /// <param name="id">The identifier of the user to edit.</param>
        /// <returns>A view containing the user edit form or a NotFound result if the user does not exist.</returns>
        [HttpGet("[Controller]/[Action]/{id}")]
        [SwaggerOperation(Summary = "Edit User Form", Description = "Displays the form for editing an existing user.")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await context.Users
                .Include(u => u.Role)
                .Include(u => u.Tasks)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            var roles = await context.Roles.ToListAsync();
            var tasks = await context.Tasks.ToListAsync();

            var viewModel = new UserEditViewModel
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Roles = roles,
                AllTasks = tasks,
                SelectedRoleId = user.RoleId,
                SelectedTasks = user.Tasks != null ? user.Tasks.Select(t => t.Id).ToArray() : []
            };

            return View(viewModel);
        }

        /// <summary>
        /// Processes the editing of an existing user, updating their information and assigned tasks.
        /// </summary>
        /// <param name="viewModel">The view model containing user edit data.</param>
        /// <returns>Redirects to the Index view upon successful editing or redisplays the form with errors.</returns>
        [HttpPost("[Controller]/[Action]/{id}")]
        [ValidateAntiForgeryToken]
        [SwaggerOperation(Summary = "Edit User", Description = "Processes the editing of an existing user.")]
        public async Task<IActionResult> Edit(UserEditViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Roles = await context.Roles.ToListAsync();
                viewModel.AllTasks = await context.Tasks.ToListAsync();
                return View(viewModel);
            }

            var user = await context.Users
                .Include(u => u.Tasks)
                .FirstOrDefaultAsync(u => u.Id == viewModel.UserId);

            if (user == null) return NotFound();

            user.Name = viewModel.Name;
            user.Email = viewModel.Email;

            if (!viewModel.SelectedRoleId.HasValue)
            {
                ModelState.AddModelError("SelectedRoleId", "Role is required.");
                viewModel.Roles = await context.Roles.ToListAsync();
                viewModel.AllTasks = await context.Tasks.ToListAsync();
                return View(viewModel);
            }

            var selectedRole = await context.Roles.FindAsync(viewModel.SelectedRoleId.Value);
            if (selectedRole == null)
            {
                ModelState.AddModelError("SelectedRoleId", "Selected role does not exist.");
                viewModel.Roles = await context.Roles.ToListAsync();
                viewModel.AllTasks = await context.Tasks.ToListAsync();
                return View(viewModel);
            }

            user.Role = selectedRole;

            user.Tasks.Clear();
            if (viewModel.SelectedTasks.Length != 0)
            {
                var selectedTasks = await context.Tasks
                    .Where(t => viewModel.SelectedTasks.Contains(t.Id))
                    .ToListAsync();
                user.Tasks.AddRange(selectedTasks);
            }

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Displays the confirmation page to delete a specific user.
        /// </summary>
        /// <param name="id">The identifier of the user to delete.</param>
        /// <returns>A view containing the delete confirmation or a NotFound result if the user does not exist.</returns>
        [HttpGet("[Controller]/[Action]/{id}")]
        [SwaggerOperation(Summary = "Delete User Confirmation", Description = "Displays the confirmation page for deleting a user.")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        /// <summary>
        /// Processes the deletion of a specific user after confirmation.
        /// </summary>
        /// <param name="id">The identifier of the user to delete.</param>
        /// <returns>Redirects to the Index view upon successful deletion or a NotFound result if the user does not exist.</returns>
        [HttpPost("[Controller]/[Action]/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [SwaggerOperation(Summary = "Delete User", Description = "Processes the deletion of a specific user.")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null) return NotFound();
            context.Users.Remove(user);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
