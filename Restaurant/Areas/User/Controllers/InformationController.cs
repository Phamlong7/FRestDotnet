using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Models;
using System.Threading.Tasks;

namespace Restaurant.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "USER")]
    public class InformationController : Controller
    {
        private readonly UserManager<UserModel> _userManager;

        public InformationController(UserManager<UserModel> userManager)
        {
            _userManager = userManager;
        }

        // Display user profile information
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // Handle email update
        [HttpPost]
        public async Task<IActionResult> UpdateEmail(string email)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Check if the email already exists in the database
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                TempData["Fail"] = "Email already exits. Try again !";
                return View("Index", user);
            }

            user.Email = email;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["Success"] = "Email updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Fail"] = "Failed to update email. Try again!";
            return View("Index", user);
        }
    }
}
