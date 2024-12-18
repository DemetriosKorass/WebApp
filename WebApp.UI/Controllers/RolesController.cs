using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.DAL;
using WebApp.DAL.Entities;

namespace WebApp.UI.Controllers
{
    public class RolesController : Controller
    {
        private readonly AppDbContext context;

        public RolesController(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await context.Roles.ToListAsync();
            return View(roles);
        }

        public IActionResult Create()
        {
            return View(new Role());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Role role, [FromForm] string[] permissions)
        {
            var combinedPermissions = permissions.Aggregate(Permissions.None, (acc, val) =>
                acc |= Enum.Parse<Permissions>(val));
            role.Permissions = combinedPermissions;

            context.Roles.Add(role);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var role = await context.Roles.FindAsync(id);
            if (role == null) return NotFound();
            return View(role);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Role updatedRole, [FromForm] string[] updatedPermissions)
        {
            if (!ModelState.IsValid) return View(updatedRole);

            var combinedPermissions = updatedPermissions.Aggregate(Permissions.None, (acc, val) =>
                acc |= Enum.Parse<Permissions>(val));
            updatedRole.Permissions = combinedPermissions;

            context.Roles.Update(updatedRole);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var role = await context.Roles.FindAsync(id);
            if (role == null) return NotFound();
            return View(role);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var role = await context.Roles.FindAsync(id);
            if (role == null) return NotFound();
            context.Roles.Remove(role);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
