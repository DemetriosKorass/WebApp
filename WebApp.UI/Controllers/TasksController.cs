using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.DAL;
using Task = WebApp.DAL.Entities.Task;

namespace WebApp.UI.Controllers
{
    public class TasksController(AppDbContext context) : Controller
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
