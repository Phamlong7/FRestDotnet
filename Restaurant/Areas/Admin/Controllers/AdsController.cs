using Microsoft.AspNetCore.Mvc;
using Restaurant.Models;
using Restaurant.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Restaurant.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/[controller]")]
    public class AdsController : Controller
    {
        private readonly DataContext _context; // Replace with your actual DbContext

        public AdsController(DataContext context)
        {
            _context = context;
        }

        // GET: admin/ads
        [HttpGet]
        public IActionResult Index()
        {
            var ads = _context.Ads.ToList();
            return View(ads);
        }

        // GET: admin/ads/create
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: admin/ads/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdsModel model)
        {
            if (ModelState.IsValid)
            {
                model.createdDate = DateTime.Now;
                model.updatedDate = null; // Set to null on creation
                _context.Ads.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Ad created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: admin/ads/edit/{id}
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(long id)
        {
            var ad = await _context.Ads.FindAsync(id);
            if (ad == null)
            {
                return NotFound();
            }
            return View(ad);
        }

        // POST: admin/ads/edit/{id}
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, AdsModel model)
        {
            if (id != model.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingAd = await _context.Ads.FindAsync(id);
                if (existingAd == null)
                {
                    return NotFound();
                }

                existingAd.images = model.images;
                existingAd.status = model.status;
                existingAd.updatedBy = model.updatedBy;
                existingAd.updatedDate = DateTime.Now;
                existingAd.width = model.width;
                existingAd.height = model.height;
                existingAd.position = model.position;
                existingAd.url = model.url;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Ad updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: admin/ads/delete/{id}
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var ad = await _context.Ads.FindAsync(id);
            if (ad == null)
            {
                return NotFound();
            }

            _context.Ads.Remove(ad);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ad deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
