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
    public class WebSettingController : Controller
    {
        private readonly DataContext _context; // Replace with your actual DbContext

        public WebSettingController(DataContext context)
        {
            _context = context;
        }

        // GET: admin/websetting
        [HttpGet]
        public IActionResult Index()
        {
            var webSettings = _context.Web_setting.ToList();
            return View(webSettings);
        }

        // GET: admin/websetting/create
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: admin/websetting/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WebSettingModel model)
        {
            if (ModelState.IsValid)
            {
                model.createdDate = DateTime.Now;
                model.updatedDate = null; // Set to null on creation
                _context.Web_setting.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Web setting created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: admin/websetting/edit/{id}
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(long id)
        {
            var webSetting = await _context.Web_setting.FindAsync(id);
            if (webSetting == null)
            {
                return NotFound();
            }
            return View(webSetting);
        }

        // POST: admin/websetting/edit/{id}
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, WebSettingModel model)
        {
            if (id != model.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingWebSetting = await _context.Web_setting.FindAsync(id);
                if (existingWebSetting == null)
                {
                    return NotFound();
                }

                existingWebSetting.content = model.content;
                existingWebSetting.updatedBy = model.updatedBy;
                existingWebSetting.updatedDate = DateTime.Now;
                existingWebSetting.status = model.status;
                existingWebSetting.type = model.type;
                existingWebSetting.image = model.image;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Web setting updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: admin/websetting/delete/{id}
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var webSetting = await _context.Web_setting.FindAsync(id);
            if (webSetting == null)
            {
                return NotFound();
            }

            _context.Web_setting.Remove(webSetting);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Web setting deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
