using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.DAL;
using WebApp.DAL.Entities;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.UI.Controllers
{


    /// <summary>
    /// Controller for managing roles, including creation, editing, and deletion.
    /// </summary>
    [Authorize]
    public class RolesController(AppDbContext context) : Controller
    {

        /// <summary>
        /// Retrieves the list of all roles.
        /// </summary>
        /// <returns>A view displaying all roles.</returns>
        [HttpGet("[Controller]")]
        [SwaggerOperation(Summary = "Get All Roles", Description = "Retrieves a list of all roles.")]
        public async Task<IActionResult> Index()
        {
            var roles = await context.Roles.ToListAsync();
            return View(roles);
        }

        /// <summary>
        /// Displays the role creation form.
        /// </summary>
        /// <returns>A view for creating a new role.</returns>
        [HttpGet("[Controller]/[Action]")]
        [SwaggerOperation(Summary = "Create Role Form", Description = "Displays the form for creating a new role.")]
        public IActionResult Create()
        {
            return View(new Role());
        }

        /// <summary>
        /// Handles the submission of a new role.
        /// </summary>
        /// <param name="role">The role entity to create.</param>
        /// <param name="permissions">An array of permissions associated with the role.</param>
        /// <returns>Redirects to the Index view upon successful creation; otherwise, returns the creation view with errors.</returns>
        [HttpPost("[Controller]/[Action]")]
        [SwaggerOperation(Summary = "Create Role", Description = "Creates a new role with the specified permissions.")]
        public async Task<IActionResult> Create(Role role, [FromForm] string[] permissions)
        {
            bool exists = await context.Roles.AnyAsync(r => r.Name == role.Name);
            if (exists)
            {
                ModelState.AddModelError("Name", "Role with this name already exists.");
                return View(role);
            }

            var combinedPermissions = permissions.Aggregate(Permissions.None, (acc, val) =>
                acc |= Enum.Parse<Permissions>(val));
            role.Permissions = combinedPermissions;

            context.Roles.Add(role);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Retrieves the role details for editing.
        /// </summary>
        /// <param name="id">The ID of the role to edit.</param>
        /// <returns>A view for editing the specified role.</returns>
        [HttpGet("[Controller]/[Action]/{id}")]
        [SwaggerOperation(Summary = "Edit Role Form", Description = "Retrieves the role details for editing.")]
        public async Task<IActionResult> Edit(int id)
        {
            var role = await context.Roles.FindAsync(id);
            if (role == null) return NotFound();
            return View(role);
        }

        /// <summary>
        /// Handles the submission of updated role information.
        /// </summary>
        /// <param name="updatedRole">The updated role entity.</param>
        /// <param name="updatedPermissions">An array of updated permissions.</param>
        /// <returns>Redirects to the Index view upon successful update; otherwise, returns the edit view with errors.</returns>
        [HttpPost("[Controller]/[Action]/{id}"), ActionName("Edit")]
        [SwaggerOperation(Summary = "Edit Role", Description = "Updates an existing role with the specified permissions.")]
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

        /// <summary>
        /// Retrieves the role details for deletion confirmation.
        /// </summary>
        /// <param name="id">The ID of the role to delete.</param>
        /// <returns>A view for confirming the deletion of the specified role.</returns>
        [HttpGet("[Controller]/[Action]/{id}")]
        [SwaggerOperation(Summary = "Delete Role Confirmation", Description = "Retrieves the role details for deletion confirmation.")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await context.Roles.FindAsync(id);
            if (role == null) return NotFound();
            return View(role);
        }

        /// <summary>
        /// Handles the confirmation of role deletion.
        /// </summary>
        /// <param name="id">The ID of the role to delete.</param>
        /// <returns>Redirects to the Index view upon successful deletion; otherwise, returns the deletion view with errors.</returns>
        [HttpPost("[Controller]/[Action]/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [SwaggerOperation(Summary = "Delete Role", Description = "Deletes the specified role.")]
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
