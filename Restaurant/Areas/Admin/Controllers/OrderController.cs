using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Repository;
using System.Linq;
using System.Threading.Tasks;

namespace Restaurant.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly DataContext _dataContext;

        public OrderController(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IActionResult> Index()
        {
            // Fetch orders along with their related user data (or other related entities)
            var order = await _dataContext.Order
                //.Include(o => o.user) // Assuming you want to include user info
                .OrderBy(o => o.id)
                .ToListAsync();

            return View(order); // Returning orders to the view instead of 'dishes'
        }
    }
}
