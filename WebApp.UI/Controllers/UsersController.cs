using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.DAL;
using WebApp.DAL.Entities;

namespace WebApp.UI.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext context;

        public UsersController(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await context.Users.ToListAsync();
            return View(users);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            if (!ModelState.IsValid) return View(user);

            context.Users.Add(user);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(User updatedUser)
        {
            if (!ModelState.IsValid) return View(updatedUser);

            context.Users.Update(updatedUser);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
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
