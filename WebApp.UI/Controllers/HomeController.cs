using Microsoft.AspNetCore.Mvc;

namespace WebApp.UI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Error()
        {
            return View();
        }
    }
}
