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
            var tasks = await context.Tasks.ToListAsync();
            return View(tasks);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Task task)
        {
            if (!ModelState.IsValid) return View(task);

            context.Tasks.Add(task);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var task = await context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            ViewBag.AllUsers = await context.Users.ToListAsync();
            return View(task);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Task updatedTask, [FromForm] int[] SelectedUsers)
        {
            var task = await context.Tasks
                .Include(t => t.Users)
                .FirstOrDefaultAsync(t => t.Id == updatedTask.Id);
            if (task == null) return NotFound();

            task.Name = updatedTask.Name;

            // Update Users
            task.Users.Clear();
            var users = await context.Users.Where(u => SelectedUsers.Contains(u.Id)).ToListAsync();
            task.Users.AddRange(users);

            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Delete(int id)
        {
            var task = await context.Tasks.FindAsync(id);
            if (task == null) return NotFound();
            return View(task);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await context.Tasks.FindAsync(id);
            if (task == null) return NotFound();
            context.Tasks.Remove(task);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
