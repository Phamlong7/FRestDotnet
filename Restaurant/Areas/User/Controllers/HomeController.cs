using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Repository;

namespace Restaurant.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "USER")]
    public class HomeController : Controller
    {
        private readonly DataContext _context;
        public HomeController(DataContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var today = DateTime.Today;
            var lastWeek = today.AddDays(-7);
            var previous7Days = lastWeek.AddDays(-7);

            // Calculate for Reports   
            var reportData = Enumerable.Range(0, 7)
                .Select(offset => today.AddDays(-offset))
                .Select(date => new
                {
                    Date = date,
                    Order = _context.orderDetails.Count(od => od.order.createdDate.HasValue && od.order.createdDate.Value.Date == date && od.order.user.UserName != null), 
                })
                .OrderBy(x => x.Date)
                .ToList();

            // Reports ViewBag
            ViewBag.Dates = reportData.Select(x => x.Date.ToString("yyyy-MM-dd")).ToList();
            ViewBag.Order = reportData.Select(x => x.Order).ToList();

            return View();
        }
    }
}
