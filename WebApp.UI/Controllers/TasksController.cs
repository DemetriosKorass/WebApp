using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.DAL;
using Task = WebApp.DAL.Entities.Task;

namespace WebApp.UI.Controllers
{
    public class TasksController : Controller
    {
        private readonly AppDbContext context;

        public TasksController(AppDbContext context)
        {
            this.context = context;
        }

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
            return View(task);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Task updatedTask)
        {
            if (!ModelState.IsValid) return View(updatedTask);

            context.Tasks.Update(updatedTask);
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
