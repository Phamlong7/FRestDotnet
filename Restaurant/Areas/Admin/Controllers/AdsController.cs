﻿using Microsoft.AspNetCore.Mvc;

namespace Restaurant.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
