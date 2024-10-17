using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using Restaurant.Repository;
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
        public async Task<IActionResult> Index()
        {
            // Retrieve the list of blogs ordered by creation date (or modify the ordering as needed)
            var blogs = await _dataContext.blog
                .Where(b => b.status == "ACTIVE") // Filter based on status if applicable
                .OrderByDescending(b => b.createdDate)
                .ToListAsync();

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
