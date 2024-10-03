using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Restaurant.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "USER")]
    public class OrderHistoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
