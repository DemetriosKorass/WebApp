using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApp.UI.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet("Account/Login")]
        [SwaggerOperation(Summary = "Login Form", Description = "Displays the login form.")]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost("Account/Login")]
        [ValidateAntiForgeryToken]
        [SwaggerOperation(Summary = "Process Login", Description = "Processes user credentials and signs in the user.")]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Dummy authentication check
            if (username == "admin" && password == "21")
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "Admin")
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity));

                return RedirectToAction("Index", "Users");
            }

            ModelState.AddModelError("", "Invalid username or password");
            return View();
        }

        // GET: /Account/Logout
        [HttpGet("Account/Logout")]
        [SwaggerOperation(Summary = "Logout User", Description = "Logs out the authenticated user.")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Users");
        }

        // GET: /Account/AccessDenied
        [HttpGet("Account/AccessDenied")]
        [SwaggerOperation(Summary = "Access Denied", Description = "Access denied page.")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
