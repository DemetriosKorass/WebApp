using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.DAL;
using WebApp.DAL.Entities;
using WebApp.UI.ViewModels;

namespace WebApp.UI.Controllers
{
    public class UsersController(AppDbContext context) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var users = await context.Users
                .Include(u => u.Role)
                .ToListAsync();
            return View(users);
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
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

        public async Task<IActionResult> Delete(int id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null) return NotFound();
            context.Users.Remove(user);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
