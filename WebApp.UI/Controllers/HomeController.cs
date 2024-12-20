using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApp.UI.Controllers
{
    /// <summary>
    /// Provides endpoints for home-related operations.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Retrieves the error message.
        /// </summary>
        /// <returns>An error message string.</returns>
        [HttpGet("[controller]/error")]
        [SwaggerOperation(Summary = "Get Error Message", Description = "Retrieves a predefined error message.")]
        [SwaggerResponse(200, "Successfully retrieved the error message.", typeof(string))]
        [SwaggerResponse(500, "Internal server error.")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
