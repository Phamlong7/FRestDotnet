using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Repository;
using Restaurant.Utility;
using Restaurant.ViewModels;

namespace Restaurant.Controllers
{
    public class AboutController : Controller
    {
        private readonly DataContext _context;
        private const string CartSessionName = "CartSession";
        private const string WishlistCookieName = "wishlist";
        public AboutController(DataContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            // Retrieve the cart from session or initialize an empty list if none exists
            var carts = HttpContext.Session.Get<List<CartItemViewModel>>(CartSessionName) ?? new List<CartItemViewModel>();
            // Set the cart count in ViewData
            ViewData["NumberCart"] = carts.Count;
            var wishlists = CookieHelper.GetCookie<List<WishlistItemViewModel>>(HttpContext, WishlistCookieName) ?? new List<WishlistItemViewModel>();
            ViewData["NumberWishList"] = wishlists.Count;

            // Get About Page
            var AboutPage = _context.web_setting
                .Where(w => w.status == "ACTIVE" && w.type == "About Page")
                .OrderByDescending(w => w.createdDate)
                .Select(w => new
                {
                    SettingID = w.id,
                    SettingType = w.type,
                    SettingImage = w.image,
                    SettingContent = w.content,
                    SettingCreatedDate = w.createdDate,
                })
                .FirstOrDefault();

            //Get the top 3 WebSetting to show the services
            var SettingServices = _context.web_setting
                .Where(w => w.status == "ACTIVE" && w.type == "Catering Services")
                .OrderByDescending(w => w.createdDate)
                .Select(w => new
                {
                    SettingID = w.id,
                    SettingType = w.type,
                    SettingImage = w.image,
                    SettingContent = w.content,
                    SettingCreatedDate = w.createdDate,
                })
                .Take(3) // Take only the top 3 after ordering
                .ToList();

            //Get the top 4 Chef for view About Page
            var TopChef = _context.web_setting
                .Where(w => w.status == "ACTIVE" && w.type == "Chef Home Page")
                .OrderByDescending(w => w.createdDate)
                .Select(w => new
                {
                    SettingID = w.id,
                    SettingType = w.type,
                    SettingImage = w.image,
                    SettingContent = w.content,
                    SettingCreatedDate = w.createdDate,
                })
                .Take(4)
                .ToList();

            //Get the top 5 Happy Customer for view about page
            var HappyCustomer = _context.web_setting
                .Where(w => w.status == "ACTIVE" && w.type == "Happy Customer")
                .OrderByDescending(w => w.createdDate)
                .Select(w => new
                {
                    SettingID = w.id,
                    SettingType = w.type,
                    SettingImage = w.image,
                    SettingContent = w.content,
                    SettingCreatedDate = w.createdDate,
                })
                .Take(5)
                .ToList();

            //ViewBag
            ViewBag.AboutPage = AboutPage;
            ViewBag.SettingServices = SettingServices;
            ViewBag.TopChef = TopChef;
            ViewBag.HappyCustomer = HappyCustomer;
            return View();
        }
    }
}
