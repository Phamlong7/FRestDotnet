using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using Restaurant.Repository;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Restaurant.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class DashboardController : Controller
    {
        private readonly DataContext _context;

        public DashboardController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var latestComments = _context.comment
                .Include(c => c.Blog)  // Ensure Blog is included
                .Where(c => c.CreatedDate >= DateTime.Now.AddDays(-7))  // Last 7 days filter
                .OrderByDescending(c => c.CreatedDate)  // Order by newest first
                .ToList();  // Retrieve all comments within the last 7 days

            var latestOrders = _context.order
                .Include(o => o.orderDetails)
                .ThenInclude(od => od.dish)
                .Where(o => o.createdDate >= DateTime.Now.AddDays(-7))
                .OrderByDescending(o => o.createdDate)
                .ToList();

            var combinedNotifications = latestComments.Cast<object>()
                .Concat(latestOrders.Cast<object>())
                .OrderByDescending(n => n is CommentModel comment ? comment.CreatedDate : (n is OrderModel order ? order.createdDate : DateTime.MinValue))
                .ToList();

            ViewData["LatestComments"] = latestComments;
            ViewData["LatestOrders"] = latestOrders;
            ViewData["CombinedNotifications"] = combinedNotifications;

            var today = DateTime.Today;
            var lastWeek = today.AddDays(-7);

            // Last 7 days data
            var last7DaysSales = _context.orderDetails
                .Where(od => od.order.createdDate >= lastWeek
                             && od.order.status == "Approved")  // Only include orders with "Approved" status
                .Sum(od => od.quantity);

            var last7DaysOrders = _context.order
                .Count(o => o.createdDate >= lastWeek
                            && (o.status == "Pending" || o.status == "Approved"));  // Count orders with either "Pending" or "Approved" status

            var last7DaysCustomers = _context.user
                .Where(u => u.CreatedDate >= lastWeek && u.Role == "USER")
                .Select(u => u.Id)
                .Distinct()
                .Count();

            // Previous 7 days data for comparison
            var previous7Days = lastWeek.AddDays(-7);

            var previous7DaysSales = _context.orderDetails
                .Where(od => od.order.createdDate >= previous7Days
                             && od.order.createdDate < lastWeek
                             && od.order.status == "Approved")  // Only include orders with "Approved" status
                .Sum(od => od.quantity);

            var previous7DaysOrders = _context.order
                .Count(o => o.createdDate >= previous7Days
                            && o.createdDate < lastWeek
                            && o.status == "Pending" || o.status == "Approved");  // Only count orders with "Approved" status


            var previous7DaysCustomers = _context.user
                .Where(u => u.CreatedDate >= previous7Days && u.CreatedDate < lastWeek && u.Role == "USER")
                .Select(u => u.Id)
                .Distinct()
                .Count();


            // Calculate percentage changes
            var salesChange = CalculatePercentageChange(previous7DaysSales, last7DaysSales);
            var ordersChange = CalculatePercentageChange(previous7DaysOrders, last7DaysOrders);
            var customersChange = CalculatePercentageChange(previous7DaysCustomers, last7DaysCustomers);

            // Calculate Report Data Charts
            var reportData = Enumerable.Range(0, 7)
                .Select(offset => today.AddDays(-offset))
                .Select(date => new
                {
                    Date = date,
                    Order = _context.order.Count(od => od.updatedDate.HasValue && od.updatedDate.Value.Date == date && od.status == "Approved"),
                    Revenue = _context.order
                        .Where(od => od.updatedDate.HasValue && od.updatedDate.Value.Date == date && od.status == "Approved")
                        .Sum(od => (decimal?)od.total) ?? 0,  // Summing the 'total' from orderDetails
                    Customers = _context.user
                        .Where(u => u.CreatedDate.Date == date && u.Role == "USER")
                        .Select(u => u.Id)
                        .Distinct()
                        .Count()
                })
                .OrderBy(x => x.Date)
                .ToList();


            // Query the orders placed 
            var recentSales = _context.order
                .Where(o => o.createdDate.Value.Date >= previous7Days
                            && o.createdDate.Value.Date <= today
                            && o.status == "Approved")  // Only include orders with status "Approved"
                .Select(o => new
                {
                    OrderId = o.id,
                    Customer = o.user.UserName,
                    Product = string.Join(", ", o.orderDetails.Select(od => od.dish.title)), // Join all dish titles
                    Price = o.total,
                    Status = o.status  // Order status
                })
                .OrderByDescending(o => o.OrderId)
                .Take(10)
                .ToList();

            // Query the top-selling products
            var topSellingProducts = _context.orderDetails
                .Where(od => od.order.updatedDate.Value.Date >= previous7Days && od.order.updatedDate.Value.Date <= today && od.order.status == "Approved")
                .GroupBy(od => new { od.dishId, od.dish.title, od.dish.price, od.dish.banner })
                .Select(g => new
                {
                    DishId = g.Key.dishId,
                    DishTitle = g.Key.title,
                    DishPrice = g.Key.price,
                    DishBanner = g.Key.banner,
                    UnitsSold = g.Sum(od => od.quantity), // Sum of quantities sold
                    Revenue = g.Sum(od => od.quantity * od.dish.price) // Calculate revenue: quantity * dish price
                })
                .OrderByDescending(p => p.UnitsSold) // Order by the number of units sold
                .Take(5)
                .ToList();

            // Query the Blog and Updates
            var blogupdates = _context.blog
                .Where(b => b.createdDate.HasValue && b.createdDate.Value.Date == today && b.status == "ACTIVE" || b.updatedDate.HasValue && b.updatedDate.Value.Date == today && b.status == "ACTIVE")
                .Select(b => new
                {
                    BlogId = b.id,
                    BlogBanner = b.banner,
                    BlogTitle = b.title,
                    BlogContent = b.content,
                    BlogStatus = b.status
                })
                .OrderByDescending(b => b.BlogId)
                .Take(10)
                .ToList();

            // ViewBag for Sales,Orders,Customer
            ViewBag.Last7DaysSales = last7DaysSales;
            ViewBag.Last7DaysOrders = last7DaysOrders;
            ViewBag.Last7DaysCustomers = last7DaysCustomers;
            ViewBag.SalesChange = salesChange;
            ViewBag.OrdersChange = ordersChange;
            ViewBag.CustomersChange = customersChange;
            // Reports ViewBag
            ViewBag.Dates = reportData.Select(x => x.Date.ToString("yyyy-MM-dd")).ToList();
            ViewBag.Order = reportData.Select(x => x.Order).ToList();
            ViewBag.Revenue = reportData.Select(x => (double)x.Revenue).ToList();
            ViewBag.Customers = reportData.Select(x => x.Customers).ToList();
            // RecentSales ViewBag
            ViewBag.RecentSales = recentSales;
            // Top Selling ViewBag
            ViewBag.TopSellingProducts = topSellingProducts;
            // Blog and Updates 
            ViewBag.BlogUpdates = blogupdates;
            return View();

        }

        private double CalculatePercentageChange(int oldValue, int newValue)
        {
            if (oldValue == 0)
            {
                return newValue > 0 ? 100 : 0;
            }
            return ((double)(newValue - oldValue) / oldValue) * 100;
        }
    }
}