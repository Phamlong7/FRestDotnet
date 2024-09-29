using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Repository;

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
            var today = DateTime.Today;
            var lastWeek = today.AddDays(-7);

            // Last 7 days data
            var last7DaysSales = _context.orderDetails
                .Where(od => od.order.createdDate >= lastWeek)
                .Sum(od => od.quantity);

            var last7DaysOrders = _context.order
                .Count(o => o.createdDate >= lastWeek);

            var last7DaysCustomers = _context.order
                .Where(o => o.createdDate >= lastWeek)
                .Select(o => o.userId)
                .Distinct()
                .Count();

            // Previous 7 days data for comparison
            var previous7Days = lastWeek.AddDays(-7);
            var previous7DaysSales = _context.orderDetails
                .Where(od => od.order.createdDate >= previous7Days && od.order.createdDate < lastWeek)
                .Sum(od => od.quantity);

            var previous7DaysOrders = _context.order
                .Count(o => o.createdDate >= previous7Days && o.createdDate < lastWeek);

            var previous7DaysCustomers = _context.order
                .Where(o => o.createdDate >= previous7Days && o.createdDate < lastWeek)
                .Select(o => o.userId)
                .Distinct()
                .Count();

            // Calculate percentage changes
            var salesChange = CalculatePercentageChange(previous7DaysSales, last7DaysSales);
            var ordersChange = CalculatePercentageChange(previous7DaysOrders, last7DaysOrders);
            var customersChange = CalculatePercentageChange(previous7DaysCustomers, last7DaysCustomers);

            // Calculate for Reports 
            var reportData = Enumerable.Range(0, 7)
                .Select(offset => today.AddDays(-offset))
                .Select(date => new
                {
                    Date = date,
                    Order = _context.orderDetails.Count(od => od.order.createdDate.HasValue && od.order.createdDate.Value.Date == date),
                    Revenue = _context.orderDetails
                        .Where(od => od.order.createdDate.HasValue && od.order.createdDate.Value.Date == date)
                        .Sum(od => od.quantity * od.price) ?? 0,
                    Customers = _context.order
                        .Where(o => o.createdDate.HasValue && o.createdDate.Value.Date == date)
                        .Select(o => o.userId)
                        .Distinct()
                        .Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            // Query the orders placed 
            var recentSales = _context.order
                .Where(o => o.createdDate >= previous7Days && o.createdDate <= today)
                .Select(o => new
                {
                    OrderId = o.id,
                    Customer = o.user.UserName,
                    Product = o.orderDetails
                                .Select(od => od.dish.title) // Dish title (from OrderDetails)
                                .FirstOrDefault(), // Assuming one product per order
                    Price = o.orderDetails
                                .Select(od => od.dish.price) // Price from Dish
                                .FirstOrDefault(),
                    Status = o.status                 // Order status
                })
                .OrderByDescending(o => o.OrderId)
                .Take(20)
                .ToList();

            // Query the top-selling products
            var topSellingProducts = _context.orderDetails
                .Where(od => od.order.createdDate >= previous7Days && od.order.createdDate <= today)
                .GroupBy(od => new { od.dishId, od.dish.title, od.dish.price, od.dish.banner })
                .Select(g => new
                {
                    DishId = g.Key.dishId,
                    DishTitle = g.Key.title,
                    DishPrice = g.Key.price,
                    DishBanner = g.Key.banner,
                    UnitsSold = g.Sum(od => od.quantity), // Sum of quantities sold
                    Revenue = g.Sum(od => od.quantity * od.price) // Total revenue for the product
                })
                .OrderByDescending(p => p.UnitsSold) // Order by the number of units sold
                .Take(5) // Limit to top 10 products
                .ToList();

            // Query the Blog and Updates
            var blogupdates = _context.blog
                .Where(b => b.createdDate.HasValue && b.createdDate.Value.Date == today && b.status == "ACTIVE")
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