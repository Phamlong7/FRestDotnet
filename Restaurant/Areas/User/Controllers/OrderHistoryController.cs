using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Restaurant.Areas.User.Controllers
{
    public class OrderHistoryController : Controller
    {
        [Area("User")]
        [Authorize(Roles = "USER")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
