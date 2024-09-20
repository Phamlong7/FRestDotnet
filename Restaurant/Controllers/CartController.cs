using Microsoft.AspNetCore.Mvc;

namespace Restaurant.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
