using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApp.UI.Controllers
{    
    /// <summary>
    /// Manages user authentication processes including login, logout, and access denial.
    /// </summary>
    public class AccountController : Controller
    {
        /// <summary>
        /// Displays the login form to the user.
        /// </summary>
        /// <returns>A view representing the login form.</returns>
        [HttpGet("Account/Login")]
        [SwaggerOperation(Summary = "Login Form", Description = "Displays the login form.")]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Processes the login credentials submitted by the user and signs in the user if the credentials are valid.
        /// </summary>
        /// <param name="username">The username provided by the user.</param>
        /// <param name="password">The password provided by the user.</param>
        /// <returns>
        /// Redirects to the Users index page upon successful login.
        /// Returns the login view with an error message if authentication fails.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when username or password is null.</exception>
        [HttpPost("[Controller]/[Action]")]
        [ValidateAntiForgeryToken]
        [SwaggerOperation(Summary = "Process Login", Description = "Processes user credentials and signs in the user.")]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username), "Username cannot be null.");

            if (password == null)
                throw new ArgumentNullException(nameof(password), "Password cannot be null.");

            // Dummy authentication check
            if (username == "admin" && password == "21")
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "Admin")
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal);

                return RedirectToAction("Index", "Users");
            }

            ModelState.AddModelError("", "Invalid username or password");
            return View();
        }

        /// <summary>
        /// Logs out the currently authenticated user and redirects to the Users index page.
        /// </summary>
        /// <returns>A redirect to the Users index view after signing out.</returns>
        [HttpGet("[Controller]/[Action]")]
        [SwaggerOperation(Summary = "Logout User", Description = "Logs out the authenticated user.")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Users");
        }

        /// <summary>
        /// Displays the access denied page when a user tries to access a restricted resource.
        /// </summary>
        /// <returns>A view indicating that access has been denied.</returns>
        [HttpGet("[Controller]/[Action]")]
        [SwaggerOperation(Summary = "Access Denied", Description = "Access denied page.")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
