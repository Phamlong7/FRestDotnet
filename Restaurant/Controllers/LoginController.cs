using Microsoft.AspNetCore.Mvc;

namespace Restaurant.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
