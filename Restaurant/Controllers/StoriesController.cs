using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using Restaurant.Repository;
using System.Linq;
using System.Threading.Tasks;

namespace Restaurant.Controllers
{
    public class StoriesController : Controller
    {
        private readonly DataContext _dataContext;

        public StoriesController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        // GET: Stories/Index
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 6; // Set the number of blogs per page

            // Retrieve the list of active blogs ordered by creation date and apply pagination
            var blogs = await _dataContext.blog
                .Where(b => b.status == "ACTIVE") // Filter based on status if applicable
                .OrderByDescending(b => b.createdDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get the total number of active blogs for pagination
            var totalBlogs = await _dataContext.blog
                .Where(b => b.status == "ACTIVE")
                .CountAsync();

            // Calculate the total number of pages
            var totalPages = (int)System.Math.Ceiling((double)totalBlogs / pageSize);

            // Pass the current page and total pages to the view for pagination controls
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(blogs);
        }

        // GET: Stories/Details/{id}
        public async Task<IActionResult> Details(long id)
        {
            var blog = await _dataContext.blog.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }
            return View(blog);
        }
    }
}