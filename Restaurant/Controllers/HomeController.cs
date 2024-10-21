using Microsoft.AspNetCore.Mvc;
using Restaurant.Models;
using Restaurant.Utility;
using Restaurant.ViewModels;
using System.Diagnostics;

namespace Restaurant.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private const string CartSessionName = "CartSession";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Retrieve the cart from session or initialize an empty list if none exists
            var carts = HttpContext.Session.Get<List<CartItemViewModel>>(CartSessionName) ?? new List<CartItemViewModel>();

            // Set the cart count in ViewData
            ViewData["NumberCart"] = carts.Count;
            return View();
        }

        public IActionResult Privacy()
        {
            // Add privacy logic here
            // Example: Check if the user is authenticated
            if (User?.Identity?.IsAuthenticated == true)
            {
                // User is authenticated, show privacy content
                return View();
            }
            else
            {
                // User is not authenticated, redirect to login page
                return RedirectToAction("Login", "Account");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
