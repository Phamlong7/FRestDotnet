using Microsoft.AspNetCore.Mvc;

namespace Restaurant.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
