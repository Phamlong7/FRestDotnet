using Microsoft.AspNetCore.Mvc;
using Restaurant.Repository;
using Restaurant.ViewModels;
using X.PagedList;
using Restaurant.Utility;
using Microsoft.AspNetCore.Authorization;

namespace Restaurant.Controllers
{
    public class MenuController : Controller
    {

        private readonly DataContext _dataContext;
        private const string CartSessionName = "CartSession";
        private const string WishlistCookieName = "wishlist";

        public MenuController(DataContext context)
        {
            _dataContext = context;
        }

        // GET: Menu/Index
        public IActionResult Index(int? page)
        {
            int pageSize = 9; // Number of dishes per page
            int pageNumber = page ?? 1; // Current page, default is page 1

            // Get list of categories and dishes from the database
            var viewModel = new MenuViewModel
            {
                // Get categories with status "ACTIVE"
                category = _dataContext.category
                    .Where(c => c.status == "ACTIVE")
                    .OrderBy(c => c.id)
                    .ToList(),

                // Get dishes with status "ACTIVE" and paginate them
                dish = _dataContext.dish
                    .Where(d => d.status == "ACTIVE")
                    .OrderBy(d => d.title)
                    .ToPagedList(pageNumber, pageSize)  // Using ToPagedList()
            };

            // Retrieve the cart from session or initialize an empty list if none exists
            var carts = HttpContext.Session.Get<List<CartItemViewModel>>(CartSessionName) ?? new List<CartItemViewModel>();
            // Set the cart count in ViewData
            ViewData["NumberCart"] = carts.Count;

            var wishlists = CookieHelper.GetCookie<List<WishlistItemViewModel>>(HttpContext, WishlistCookieName) ?? new List<WishlistItemViewModel>();
            ViewData["NumberWishList"] = wishlists.Count;

            return View(viewModel);
        }

        // GET: Menu/DishesByCategory
        public IActionResult DishesByCategory(long categoryId, int? page)
        {
            int pageSize = 9; // Số món ăn mỗi trang
            int pageNumber = page ?? 1; // Trang hiện tại, mặc định là trang 1

            var viewModel = new MenuViewModel
            {
                category = _dataContext.category
                    .Where(c => c.status == "ACTIVE")
                    .OrderBy(c => c.id)
                    .ToList(),

                dish = _dataContext.dish
                    .Where(d => d.categoryId == categoryId && d.status == "ACTIVE")
                    .OrderBy(d => d.title)
                    .ToPagedList(pageNumber, pageSize)
            };

            ViewData["CurrentCategoryId"] = categoryId; // Lưu categoryId để dùng trong view

            return View("Index", viewModel);
        }

    }
}