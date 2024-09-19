using Microsoft.AspNetCore.Mvc;

namespace Restaurant.Areas.Admin.Controllers
{
    public class AdsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
