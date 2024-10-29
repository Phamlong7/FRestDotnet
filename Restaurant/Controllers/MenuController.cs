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

            return View(viewModel);
        }

        // GET: Menu/DishesByCategory
        public IActionResult DishesByCategory(long categoryId, int? page)
        {
            int pageSize = 9; // Number of dishes per page
            int pageNumber = page ?? 1; // Current page, default is page 1

            // Get list of dishes by category with status "ACTIVE"
            var viewModel = new MenuViewModel
            {
                // Get categories with status "ACTIVE"
                category = _dataContext.category
                    .Where(c => c.status == "ACTIVE")
                    .OrderBy(c => c.id)
                    .ToList(),

                // Get dishes by category with status "ACTIVE"
                dish = _dataContext.dish
                    .Where(d => d.categoryId == categoryId && d.status == "ACTIVE")
                    .OrderBy(d => d.title)
                    .ToPagedList(pageNumber, pageSize)  // Using ToPagedList()
            };

            return View("Index", viewModel);
        }
    }
}