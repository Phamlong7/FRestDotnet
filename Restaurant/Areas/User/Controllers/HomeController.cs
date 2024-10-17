using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Models;
using Restaurant.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Restaurant.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "USER")]
    public class HomeController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<UserModel> _userManager;

        public HomeController(DataContext context, UserManager<UserModel> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Get the current logged-in user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            var today = DateTime.Today;
            var lastWeek = today.AddDays(-7);
            var previous7Days = lastWeek.AddDays(-7);

            // Calculate reports only for the current logged-in user
            var reportData = Enumerable.Range(0, 7)
                .Select(offset => today.AddDays(-offset))
                .Select(date => new
                {
                    Date = date,
                    Order = _context.order.Count(o => o.createdDate.HasValue
                                                              && o.createdDate.Value.Date == date
                                                              && o.userId == currentUser.Id), // Filter by logged-in user's orders
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