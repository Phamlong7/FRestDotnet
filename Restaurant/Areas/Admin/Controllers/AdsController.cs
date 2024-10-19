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

        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.ads.OrderBy(a => a.id).ToListAsync());
        }

        // GET: ads/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ads/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdsModel ad)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (ad.imageUpload != null) // Check if an image is uploaded
                    {
                        // Save the uploaded image and get the URL
                        ad.url = await _fileService.SaveFile(ad.imageUpload, "Media", new string[] { ".jpg", ".jpeg", ".png" });
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error uploading image: " + ex.Message);
                    return View(ad);
                }

                var user = await _userManager.GetUserAsync(User);
                ad.createdBy = user?.UserName; // Set the creator's username

                _dataContext.ads.Add(ad); // Add the ad entry to the context
                await _dataContext.SaveChangesAsync(); // Save changes to the database

                // Set success message in TempData
                TempData["SuccessMessage"] = "Ad created successfully!";

                return RedirectToAction("Index"); // Redirect to the index page on success
            }

            return View(ad); // Return the form with validation errors if model state is invalid
        }

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

                    // Update other fields
                    existingAd.status = ad.status;
                    existingAd.width = ad.width;
                    existingAd.height = ad.height;
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

        public async Task<IActionResult> Delete(long id)
        {
            var ad = await _dataContext.ads.FindAsync(id);
            if (ad == null)
            {
                return NotFound();
            }

            // Delete the associated image if it exists
            if (!string.IsNullOrWhiteSpace(ad.url))
            {
                try
                {
                    // Attempt to delete the file from the Media folder
                    _fileService.DeleteFile(ad.url, "Media");
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
            _dataContext.ads.Remove(ad);
            await _dataContext.SaveChangesAsync();

            // Set success message in TempData
            TempData["SuccessMessage"] = "Web setting deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}