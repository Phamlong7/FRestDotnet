using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Repository;

namespace Restaurant.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DishController : Controller
    {
        private readonly DataContext _dataContext;

        public DishController(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IActionResult> Index()
        {
            // Fetch dishes along with their related category data (if needed)
            var dishes = await _dataContext.dish
                .Include(d => d.category)  // Include the category if you want to show category info in the view
                .OrderBy(d => d.id)
                .ToListAsync();

            return View(dishes);
        }

    }
}
