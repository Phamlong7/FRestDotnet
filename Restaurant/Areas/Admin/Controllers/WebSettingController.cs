using Microsoft.AspNetCore.Mvc;

namespace Restaurant.Areas.Admin.Controllers
{
    public class WebSettingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
