using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Repository;

namespace Restaurant.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class WebSettingController : Controller
    {
        private readonly DataContext _dataContext;

        public WebSettingController(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.web_setting.OrderBy(w => w.id).ToListAsync());
        }
    }
}
