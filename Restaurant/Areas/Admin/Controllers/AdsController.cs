using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using Restaurant.Repository;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace Restaurant.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class AdsController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<UserModel> _userManager;
        private readonly IFileService _fileService;

        public AdsController(DataContext context, UserManager<UserModel> userManager, IFileService fileService)
        {
            _dataContext = context;
            _userManager = userManager;
            _fileService = fileService;
        }

        // GET: Admin/Ads
        public async Task<IActionResult> Index()
        {
            var ads = await _dataContext.ads.OrderBy(a => a.id).ToListAsync();

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

            return View(ads);
        }

        // GET: Admin/Ads/Create
        public IActionResult Create()
        {
            var ads = new AdsModel();
            return View(ads);
        }

        // POST: Admin/Ads/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdsModel ad)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if an image is uploaded
                    if (ad.imageUpload != null)
                    {
                        ad.url = await _fileService.SaveFile(ad.imageUpload, "Media", new string[] { ".jpg", ".jpeg", ".png" });
                    }

                    // Capture user info
                    var user = await _userManager.GetUserAsync(User);
                    ad.createdBy = user?.UserName;
                    ad.createdDate = DateTime.Now;

                    // Add the ad to the database
                    _dataContext.ads.Add(ad);
                    await _dataContext.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Ad created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error uploading image: " + ex.Message);
                }
            }
            return View(ad);
        }

        // GET: Admin/Ads/Edit/{id}
        public async Task<IActionResult> Edit(long id)
        {
            var existingAd = await _dataContext.ads.FindAsync(id);
            if (existingAd == null)
            {
                return NotFound();
            }
            return View(existingAd);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, AdsModel ad)
        {
            if (ModelState.IsValid)
            {
                // Find the existing ad in the database
                var existingAd = await _dataContext.ads.FindAsync(id);
                if (existingAd == null)
                {
                    return NotFound();
                }

                string? oldImage = existingAd.url; // Store the old image URL
                try
                {
                    // Check if a new image is uploaded
                    if (ad.imageUpload != null)
                    {
                        // Save the new image and update the URL
                        existingAd.url = await _fileService.SaveFile(ad.imageUpload, "Media", new string[] { ".jpg", ".jpeg", ".png" });

                        // Delete the old image if it exists
                        if (!string.IsNullOrWhiteSpace(oldImage))
                        {
                            _fileService.DeleteFile(oldImage, "Media"); // Delete the old image
                        }
                    }

                    // Update other fields including width and height
                    existingAd.status = ad.status;
                    existingAd.width = ad.width; // Set width
                    existingAd.height = ad.height; // Set height
                    existingAd.position = ad.position;

                    // Keep the image URL the same if no new image is uploaded
                    if (ad.imageUpload == null)
                    {
                        existingAd.url = oldImage;
                    }

                    var user = await _userManager.GetUserAsync(User);
                    existingAd.updatedBy = user?.UserName; // Set the updater's username
                    existingAd.updatedDate = DateTime.Now;

                    await _dataContext.SaveChangesAsync(); // Save changes to the database

                    // Set success message in TempData
                    TempData["SuccessMessage"] = "Ad edited successfully!";

                    return RedirectToAction("Index"); // Redirect to the index page on success
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error uploading image: " + ex.Message);
                    return View(ad);
                }
            }

            return View(ad); // Return the form with validation errors if model state is invalid
        }

        // GET: Admin/Ads/Delete/{id}
        [HttpPost]
        public IActionResult DeleteConfirmed(long id)
        {
            // Find the ad
            var ad = _dataContext.ads.Find(id);
            if (ad != null)
            {
                _dataContext.ads.Remove(ad); // Remove the ad
                _dataContext.SaveChanges(); // Commit changes to the database
                TempData["SuccessMessage"] = "Ad deleted successfully."; // Set success message
            }
            else
            {
                TempData["ErrorMessage"] = "Ad not found."; // Set error message if ad not found
            }

            return RedirectToAction("Index"); // Redirect after deletion
        }

        // GET: Admin/Ads/RandomAd
        [HttpGet]
        public IActionResult RandomAd()
        {
            var randomAd = _dataContext.ads.OrderBy(a => Guid.NewGuid()).FirstOrDefault(); // Get a random ad
            if (randomAd == null)
            {
                return NotFound();
            }

            return Ok(randomAd); // Return the random ad as JSON
        }
    }
}
