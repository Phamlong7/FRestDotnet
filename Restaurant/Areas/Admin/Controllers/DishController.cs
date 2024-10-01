using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using Restaurant.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Restaurant.Areas.Admin;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Restaurant.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DishController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<UserModel> _userManager;
        private readonly IFileService _fileService;

        public DishController(DataContext context, UserManager<UserModel> userManager, IFileService fileService)
        {
            _dataContext = context;
            _userManager = userManager;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.dish.Include(d => d.category).OrderBy(d => d.id).ToListAsync());
        }

        // GET: Dish/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_dataContext.category, "id", "name");
            return View();
        }

        // POST: Dish/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DishModel dish)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (dish.BannerUpload != null)
                    {
                        dish.banner = await _fileService.SaveFile(dish.BannerUpload, "Media", new string[] { ".jpg", ".jpeg", ".png" });
                    }
                }
                catch (Exception ex)
                {
                    // Handle exception
                }
                var user = await _userManager.GetUserAsync(User);
                dish.createdBy = user.UserName;
                dish.createdDate = DateTime.Now;
                _dataContext.dish.Add(dish);
                await _dataContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Dish created successfully!";
                return RedirectToAction("Index");
            }
            ViewData["CategoryId"] = new SelectList(_dataContext.category, "id", "name", dish.categoryId);
            return View(dish);
        }

        // GET: Dish/Edit/5
        public async Task<IActionResult> Edit(long id)
        {
            var dish = await _dataContext.dish.FindAsync(id);
            if (dish == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_dataContext.category, "id", "name", dish.categoryId);
            return View(dish);
        }

        // POST: Dish/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, DishModel dish)
        {
            if (ModelState.IsValid)
            {
                var existingDish = await _dataContext.dish.FindAsync(id);
                if (existingDish == null)
                {
                    return NotFound();
                }

                string? oldBanner = existingDish.banner;
                try
                {
                    if (dish.BannerUpload != null)
                    {
                        existingDish.banner = await _fileService.SaveFile(dish.BannerUpload, "Media", new string[] { ".jpg", ".jpeg", ".png" });
                        if (!string.IsNullOrWhiteSpace(oldBanner))
                        {
                            _fileService.DeleteFile(oldBanner, "Media");
                        }
                    }
                    else
                    {
                        existingDish.banner = oldBanner;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error uploading banner: " + ex.Message);
                    return View(dish);
                }

                var user = await _userManager.GetUserAsync(User);
                existingDish.updatedBy = user.UserName;
                existingDish.updatedDate = DateTime.Now;
                existingDish.title = dish.title;
                existingDish.content = dish.content;
                existingDish.price = dish.price;
                existingDish.categoryId = dish.categoryId;
                existingDish.status = dish.status;

                await _dataContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Dish updated successfully!";
                return RedirectToAction("Index");
            }
            ViewData["CategoryId"] = new SelectList(_dataContext.category, "id", "name", dish.categoryId);
            return View(dish);
        }

        // GET: Dish/Delete/5
        public async Task<IActionResult> Delete(long id)
        {
            var dish = await _dataContext.dish.FindAsync(id);
            if (dish == null)
            {
                return NotFound();
            }
            _dataContext.dish.Remove(dish);
            await _dataContext.SaveChangesAsync();
            if (!string.IsNullOrWhiteSpace(dish.banner))
            {
                _fileService.DeleteFile(dish.banner, "Media");
            }
            TempData["SuccessMessage"] = "Dish deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}
