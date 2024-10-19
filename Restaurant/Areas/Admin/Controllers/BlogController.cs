using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using Restaurant.Repository;
using Microsoft.AspNetCore.Identity;
using Restaurant.Areas.Admin;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Authorization;

namespace Restaurant.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class BlogController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<UserModel> _userManager;
        private readonly IFileService _fileService;

        public BlogController(DataContext context, UserManager<UserModel> userManager, IFileService fileService)
        {
            _dataContext = context;
            _userManager = userManager;
            _fileService = fileService;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.blog.OrderBy(b => b.id).ToListAsync());
        }
        // GET: Blog/Create
        public IActionResult Create()
        {
            return View();
        }
        // POST: Blog/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BlogModel blog)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (blog.BannerUpload != null)
                    {
                        blog.banner = await _fileService.SaveFile(blog.BannerUpload, "Media", new string[] { ".jpg", ".jpeg", ".png" });
                    }
                }
                catch (Exception ex)
                {
                }
                var user = await _userManager.GetUserAsync(User);
                blog.createdBy = user.UserName;
                _dataContext.blog.Add(blog); // Add the blog entry to the context
                await _dataContext.SaveChangesAsync(); // Save changes to the database
                // Set success message in TempData
                TempData["SuccessMessage"] = "Blog created successfully!";
                return RedirectToAction("Index"); // Redirect to the index page on success
            }
            return View(blog); // Return the form with validation errors if model state is invalid
        }
        public async Task<IActionResult> Edit(long id)
        {
            var existingBlog = await _dataContext.blog.FindAsync(id);
            if (existingBlog == null)
            {
                return NotFound();
            }
            return View(existingBlog);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, BlogModel blog)
        {
            if (ModelState.IsValid)
            {
                var existingBlog = await _dataContext.blog.FindAsync(id);
                if (existingBlog == null)
                {
                    return NotFound();
                }
                string? oldbanner = existingBlog.banner;
                try
                {
                    if (blog.BannerUpload != null)
                    {
                        existingBlog.banner = await _fileService.SaveFile(blog.BannerUpload, "Media", new string[] { ".jpg", ".jpeg", ".png" });
                        if (!string.IsNullOrWhiteSpace(oldbanner))
                        {
                            _fileService.DeleteFile(oldbanner, "Media");
                        }
                    }
                    else
                    {
                        existingBlog.banner = oldbanner;
                    }
                }
                catch (Exception ex) //handle something
                {
                    // Handle exception (e.g., log it, show error message)
                    ModelState.AddModelError("", "Error uploading banner: " + ex.Message);
                    return View(blog);
                }
                existingBlog.createdBy = existingBlog.createdBy;
                existingBlog.createdDate = existingBlog.createdDate;
                var user = await _userManager.GetUserAsync(User);
                existingBlog.updatedBy = user.UserName;
                existingBlog.updatedDate = DateTime.Now;
                existingBlog.content = blog.content;
                existingBlog.status = blog.status;
                existingBlog.title = blog.title;
                await _dataContext.SaveChangesAsync(); // Save changes to the database
                // Set success message in TempData
                TempData["SuccessMessage"] = "Blog edited successfully!";
                return RedirectToAction("Index"); // Redirect to the index page on success
            }
            return View(blog); // Return the form with validation errors if model state is invalid
        }
        public async Task<IActionResult> Delete(long id)
        {
            var blog = await _dataContext.blog.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }
            _dataContext.blog.Remove(blog);
            await _dataContext.SaveChangesAsync();
            if (!string.IsNullOrWhiteSpace(blog.banner))
            {
                _fileService.DeleteFile(blog.banner, "Media");
            }
            // Set success message in TempData
            TempData["SuccessMessage"] = "Blog edited successfully!";
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Details(long id)
        {
            var blog = await _dataContext.blog.FindAsync(id);
            if (blog == null)
            {
                return NotFound(); // Return 404 if the blog is not found
            }
            return View(blog); // Pass the blog to the view
        }
    }
}