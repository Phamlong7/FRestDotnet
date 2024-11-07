using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using Restaurant.Repository;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Restaurant.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class WebSettingController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<UserModel> _userManager;
        private readonly IFileService _fileService;

        public WebSettingController(DataContext context, UserManager<UserModel> userManager, IFileService fileService)
        {
            _dataContext = context;
            _userManager = userManager;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index()
        {
            var latestComments = _dataContext.comment
                .Include(c => c.Blog)  // Ensure Blog is included
                .Where(c => c.CreatedDate >= DateTime.Now.AddDays(-7))  // Last 7 days filter
                .OrderByDescending(c => c.CreatedDate)  // Order by newest first
                .ToList();  // Retrieve all comments within the last 7 days

            var latestOrders = _dataContext.order
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

            return View(await _dataContext.web_setting.OrderBy(a => a.id).ToListAsync());
        }

        // GET: WebSettings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: WebSettings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WebSettingModel webSetting)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (webSetting.imageUpload != null) // Check if an image is uploaded
                    {
                        // Save the uploaded image and get the URL
                        webSetting.image = await _fileService.SaveFile(webSetting.imageUpload, "Media", new string[] { ".jpg", ".jpeg", ".png" });
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error uploading image: " + ex.Message);
                    return View(webSetting);
                }

                var user = await _userManager.GetUserAsync(User);
                
                //Beware of createdBy
                //webSetting.createdBy = user?.UserName; // Set the creator's username

                _dataContext.web_setting.Add(webSetting); // Add the web setting entry to the context
                await _dataContext.SaveChangesAsync(); // Save changes to the database

                // Set success message in TempData
                TempData["SuccessMessage"] = "Web setting created successfully!";

                return RedirectToAction("Index"); // Redirect to the index page on success
            }

            return View(webSetting); // Return the form with validation errors if model state is invalid
        }

        public async Task<IActionResult> Edit(long id)
        {
            var existingSetting = await _dataContext.web_setting.FindAsync(id);
            if (existingSetting == null)
            {
                return NotFound();
            }
            return View(existingSetting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, WebSettingModel webSetting)
        {
            if (ModelState.IsValid)
            {
                var existingSetting = await _dataContext.web_setting.FindAsync(id);
                if (existingSetting == null)
                {
                    return NotFound();
                }

                string? oldImage = existingSetting.image; // Store the old image URL
                try
                {
                    if (webSetting.imageUpload != null) // Check if a new image is uploaded
                    {
                        // Save the new image and update the URL
                        existingSetting.image = await _fileService.SaveFile(webSetting.imageUpload, "Media", new string[] { ".jpg", ".jpeg", ".png" });

                        // Delete the old image if it exists
                        if (!string.IsNullOrWhiteSpace(oldImage))
                        {
                            _fileService.DeleteFile(oldImage, "Media"); // Delete the old image
                        }
                    }

                    // Update other fields
                    existingSetting.content = webSetting.content;
                    existingSetting.status = webSetting.status;
                    existingSetting.type = webSetting.type;

                    var user = await _userManager.GetUserAsync(User);
                    existingSetting.updatedBy = user?.UserName; // Set the updater's username
                    existingSetting.updatedDate = DateTime.Now;

                    await _dataContext.SaveChangesAsync(); // Save changes to the database

                    // Set success message in TempData
                    TempData["SuccessMessage"] = "Web setting edited successfully!";

                    return RedirectToAction("Index"); // Redirect to the index page on success
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error uploading image: " + ex.Message);
                    return View(webSetting);
                }
            }

            return View(webSetting); // Return the form with validation errors if model state is invalid
        }

        public async Task<IActionResult> Delete(long id)
        {
            var webSetting = await _dataContext.web_setting.FindAsync(id);
            if (webSetting == null)
            {
                return NotFound();
            }

            // Delete the associated image if it exists
            if (!string.IsNullOrWhiteSpace(webSetting.image))
            {
                try
                {
                    // Attempt to delete the file from the Media folder
                    _fileService.DeleteFile(webSetting.image, "Media");
                }
                catch (FileNotFoundException ex)
                {
                    // Handle the case where the file doesn't exist (optional)
                    TempData["ErrorMessage"] = ex.Message;
                }
                catch (Exception ex)
                {
                    // Handle other exceptions (e.g., permission issues)
                    TempData["ErrorMessage"] = "Error deleting the image: " + ex.Message;
                }
            }

            // Remove the web setting from the database
            _dataContext.web_setting.Remove(webSetting);
            await _dataContext.SaveChangesAsync();

            // Set success message in TempData
            TempData["SuccessMessage"] = "Web setting deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}
