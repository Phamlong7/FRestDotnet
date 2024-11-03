using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Restaurant.Utility;
using Restaurant.ViewModels;

namespace Restaurant.Controllers
{
    public class WishlistController : Controller
    {
        private const string WishlistCookieName = "wishlist";
        private const string CartSessionName = "CartSession";


        private List<WishlistItemViewModel> GetWishlistFromCookies()
        {
            return CookieHelper.GetCookie<List<WishlistItemViewModel>>(HttpContext, WishlistCookieName) ?? new List<WishlistItemViewModel>();
        }

        private void SetWishlistInCookies(List<WishlistItemViewModel> wishlist)
        {
            CookieHelper.SetCookie(HttpContext, WishlistCookieName, wishlist, 30);
        }

        [HttpPost]
        public IActionResult AddToWishlist([FromBody] WishlistItemViewModel item)
        {
            if (item == null)
            {
                return BadRequest("Invalid item.");
            }

            var wishlist = GetWishlistFromCookies();
            if (!wishlist.Any(w => w.DishId == item.DishId))
            {
                wishlist.Add(item);
                SetWishlistInCookies(wishlist);
            }

            return Ok(wishlist.Count);
        }

        [HttpPost]
        public IActionResult RemoveFromWishlist([FromBody] long dishId)
        {
            var wishlist = GetWishlistFromCookies();
            var itemToRemove = wishlist.FirstOrDefault(w => w.DishId == dishId);
            if (itemToRemove != null)
            {
                wishlist.Remove(itemToRemove); // Remove from wishlist
                SetWishlistInCookies(wishlist); // Update the cookie
            }

            return Ok(wishlist.Count); // Return the updated wishlist
        }

        // New action method to remove items from the view
        [HttpPost]
        public IActionResult RemoveFromView(long dishId)
        {
            var wishlist = GetWishlistFromCookies();
            var itemToRemove = wishlist.FirstOrDefault(w => w.DishId == dishId);
            if (itemToRemove != null)
            {
                wishlist.Remove(itemToRemove); // Remove from wishlist
                SetWishlistInCookies(wishlist); // Update the cookie
            }

            // Redirect back to the wishlist index view after removal
            return RedirectToAction("Index", wishlist.Count);
        }

        [HttpPost]
        public IActionResult ClearWishlist()
        {
            SetWishlistInCookies(new List<WishlistItemViewModel>()); // Set an empty list in the cookie
            return Ok(); // Return a 200 response indicating success
        }

        public IActionResult Index()
        {
            var wishlist = GetWishlistFromCookies();
            ViewData["NumberWishlist"] = wishlist.Count; // Set the count in ViewData

            // Retrieve the cart from session or initialize an empty list if none exists
            var carts = HttpContext.Session.Get<List<CartItemViewModel>>(CartSessionName) ?? new List<CartItemViewModel>();
            // Set the cart count in ViewData
            ViewData["NumberCart"] = carts.Count;
            return View(wishlist);
        }
    }
}
