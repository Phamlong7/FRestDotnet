using Microsoft.AspNetCore.Mvc;
using Restaurant.Utility;
using Restaurant.ViewModels;

namespace Restaurant.Controllers
{
    public class AboutController : Controller
    {
        private const string CartSessionName = "CartSession";
        public IActionResult Index()
        {
            // Retrieve the cart from session or initialize an empty list if none exists
            var carts = HttpContext.Session.Get<List<CartItemViewModel>>(CartSessionName) ?? new List<CartItemViewModel>();

            // Set the cart count in ViewData
            ViewData["NumberCart"] = carts.Count;
            return View();
        }
    }
}
