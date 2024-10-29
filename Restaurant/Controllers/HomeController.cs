using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Restaurant.Models;
using Restaurant.Repository;
using Restaurant.Utility;
using Restaurant.ViewModels;

namespace Restaurant.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext _context;
        private const string CartSessionName = "CartSession";

        public HomeController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Retrieve the cart from session or initialize an empty list if none exists
            var carts = HttpContext.Session.Get<List<CartItemViewModel>>(CartSessionName) ?? new List<CartItemViewModel>();
            // Set the cart count in ViewData
            ViewData["NumberCart"] = carts.Count;

            // Get the top 3 WebSetting for the HomePage, ordered by creation date
            var SettingHomePage = _context.web_setting
                .Where(w => w.status == "ACTIVE" && w.type == "Welcome To Our Restaurant")
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

            //Get the top 4 news dishes for the top of the page
            var NewDishes = _context.dish
                .Where(d => d.status == "ACTIVE")
                .Select(d => new
                {
                    DishId = d.id,
                    DishTitle = d.title,
                    DishContent = d.content,
                    DishPrice = d.price,
                    DishBanner = d.banner,
                    DishCreatedDate = d.createdDate,
                })
                .Take(4)
                .OrderByDescending(d => d.DishCreatedDate)
                .ToList();

            //Get the top 4 Chef for view Home Page
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

            // Get the top 6 approved dishes
            var TopDishes = _context.dish
                .Where(d => d.status == "ACTIVE")
                .OrderByDescending(d => d.orderDetails.Sum(od => od.quantity))
                .Select(d => new
                {
                    DishId = d.id,
                    DishTitle = d.title,
                    DishContent = d.content,
                    DishPrice = d.price,
                    DishBanner = d.banner,
                    DishCreatedDate = d.createdDate,
                })
                .Take(6)
                .ToList();

         
            // Get the top 3 most recent active blogs
            var TopBlog = _context.blog
                .Where(b => b.status == "ACTIVE")
                .OrderByDescending(b => b.createdDate) // Sort by created date, newest first
                .Select(b => new
                {
                    BlogId = b.id,
                    BlogTitle = b.title,
                    BlogContent = b.content,
                    BlogBanner = b.banner,
                    BlogCreatedDate = b.createdDate
                })
                .Take(3) // Take the top 3 after sorting
                .ToList();

            // Pass the data to the view
            ViewBag.SettingHomePage = SettingHomePage;
            ViewBag.SettingServices = SettingServices;
            ViewBag.NewDishes = NewDishes;
            ViewBag.TopChef = TopChef;
            ViewBag.TopDishes = TopDishes;
            ViewBag.TopBlog = TopBlog;           
            return View();
        }

    }
}
