using Microsoft.AspNetCore.Mvc;

namespace Restaurant.Controllers
{
    public class StoriesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
