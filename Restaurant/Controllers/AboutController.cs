using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Utility;
using Restaurant.ViewModels;

namespace Restaurant.Controllers
{
    public class AboutController : Controller
    {
        private const string CartSessionName = "CartSession";
        private const string WishlistCookieName = "wishlist";
        public IActionResult Index()
        {
            // Retrieve the cart from session or initialize an empty list if none exists
            var carts = HttpContext.Session.Get<List<CartItemViewModel>>(CartSessionName) ?? new List<CartItemViewModel>();
            // Set the cart count in ViewData
            ViewData["NumberCart"] = carts.Count;
            var wishlists = CookieHelper.GetCookie<List<WishlistItemViewModel>>(HttpContext, WishlistCookieName) ?? new List<WishlistItemViewModel>();
            ViewData["NumberWishList"] = wishlists.Count;
            return View();
        }
    }
}
