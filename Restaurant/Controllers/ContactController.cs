using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Models;
using Restaurant.Utility;
using Restaurant.ViewModels;
using System.Threading.Tasks;

namespace Restaurant.Controllers
{
    [Authorize(Roles = "USER, ADMIN")]
    public class ContactController : Controller
    {
        private readonly SendMail _sendMail;
        private readonly UserManager<UserModel> _userManager;
        private readonly ConstantHelper _constantHelper;
        private const string CartSessionName = "CartSession";
        private const string WishlistCookieName = "wishlist";

        public ContactController(SendMail sendMail, UserManager<UserModel> userManager, ConstantHelper constantHelper)
        {
            _sendMail = sendMail;
            _userManager = userManager;
            _constantHelper = constantHelper;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                ViewBag.UserName = currentUser.UserName; // Assuming UserName contains the user's full name
                ViewBag.UserEmail = currentUser.Email; // Get the user's email
            }
            // Retrieve the cart from session or initialize an empty list if none exists
            var carts = HttpContext.Session.Get<List<CartItemViewModel>>(CartSessionName) ?? new List<CartItemViewModel>();
            // Set the cart count in ViewData
            ViewData["NumberCart"] = carts.Count;

            var wishlists = CookieHelper.GetCookie<List<WishlistItemViewModel>>(HttpContext, WishlistCookieName) ?? new List<WishlistItemViewModel>();
            ViewData["NumberWishList"] = wishlists.Count;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendContactMessage(string name, string email, string subject, string message)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    // Prepare the email body
                    string body = $"<strong>Name:</strong> {name}<br/>" +
                                  $"<strong>Email:</strong> {email}<br/>" +
                                  $"<strong>Message:</strong><br/>{message}";

                    // Get the host email from ConstantHelper
                    string hostEmail = _constantHelper.emailsender; // Use the sender email

                    // Send the email
                    bool emailSent = await _sendMail.SendEmailAsync(
                        hostEmail, // Email to send to (configured in ConstantHelper)
                        subject,
                        body
                    );

                    if (emailSent)
                    {
                        TempData["SuccessMessage"] = "Your message has been sent successfully.";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "There was an error sending your message. Please try again.");
                    }
                }
            }

            return View("Index");
        }
    }
}
