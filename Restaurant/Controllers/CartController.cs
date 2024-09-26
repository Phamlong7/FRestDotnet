using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Restaurant.Controllers
{
    [Authorize(Roles = "USER, ADMIN")]
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
