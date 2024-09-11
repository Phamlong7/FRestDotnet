using Microsoft.AspNetCore.Mvc;

namespace Restaurant.Controllers
{
    public class ReservationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
