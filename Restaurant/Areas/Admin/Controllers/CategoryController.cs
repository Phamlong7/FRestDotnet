using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using Restaurant.Repository;
using Microsoft.AspNetCore.Identity;
using Restaurant.Areas.Admin;
using Microsoft.AspNetCore.Authorization;

namespace Restaurant.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<UserModel> _userManager;

        public CategoryController(DataContext context, UserManager<UserModel> userManager)
        {
            _dataContext = context;
            _userManager = userManager;
        }

        // GET: Category/Index
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.category.OrderBy(c => c.id).ToListAsync());
        }

        // GET: Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                category.createdBy = user?.UserName;
                category.createdDate = DateTime.Now;

                _dataContext.category.Add(category);
                await _dataContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Category created successfully!";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(long id)
        {
            var category = await _dataContext.category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                var existingCategory = await _dataContext.category.FindAsync(id);
                if (existingCategory == null)
                {
                    return NotFound();
                }

                var user = await _userManager.GetUserAsync(User);
                existingCategory.name = category.name;
                existingCategory.description = category.description;
                existingCategory.status = category.status;
                existingCategory.updatedBy = user.UserName;
                existingCategory.updatedDate = DateTime.Now;

                await _dataContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Category updated successfully!";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(long id)
        {
            var category = await _dataContext.category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _dataContext.category.Remove(category);
            await _dataContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Category deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}
