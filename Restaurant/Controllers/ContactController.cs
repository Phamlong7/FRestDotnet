using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Models;
using Restaurant.Repository;
using Restaurant.Utility;
using Restaurant.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Restaurant.Controllers
{
    [Authorize(Roles = "USER, ADMIN")]
    public class ContactController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly SendMail _sendMail;
        private readonly UserManager<UserModel> _userManager;
        private readonly ConstantHelper _constantHelper;
        private const string CartSessionName = "CartSession";
        private const string WishlistCookieName = "wishlist";

        public ContactController(SendMail sendMail, UserManager<UserModel> userManager, ConstantHelper constantHelper, DataContext dataContext)
        {
            _sendMail = sendMail;
            _userManager = userManager;
            _constantHelper = constantHelper;
            _dataContext = dataContext;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                ViewBag.UserName = currentUser.UserName; // Giả sử UserName chứa tên đầy đủ của người dùng
                ViewBag.UserEmail = currentUser.Email; // Lấy email của người dùng
            }

            // Lấy ảnh quảng cáo từ bảng Ads
            var adImage = _dataContext.ads
                .Where(ad => ad.status == "active") // Chỉ lấy quảng cáo đang hoạt động
                .OrderByDescending(ad => ad.createdDate) // Sắp xếp theo ngày tạo giảm dần
                .Select(ad => ad.url) // Chọn trường URL của ảnh
                .FirstOrDefault();

            ViewBag.AdImageFileName = adImage; // Truyền ảnh quảng cáo vào ViewBag

            // Lấy giỏ hàng từ session hoặc khởi tạo danh sách trống nếu không tồn tại
            var carts = HttpContext.Session.Get<List<CartItemViewModel>>(CartSessionName) ?? new List<CartItemViewModel>();

            // Set số lượng giỏ hàng vào ViewData
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
                    // Chuẩn bị nội dung email
                    string body = $"<strong>Name:</strong> {name}<br/>" +
                                  $"<strong>Email:</strong> {email}<br/>" +
                                  $"<strong>Message:</strong><br/>{message}";

                    // Lấy email của người gửi từ ConstantHelper
                    string hostEmail = _constantHelper.emailsender; // Sử dụng email đã cấu hình trong ConstantHelper

                    // Gửi email
                    bool emailSent = await _sendMail.SendEmailAsync(
                        hostEmail, // Email gửi đến (được cấu hình trong ConstantHelper)
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

            return View("Index"); // Trả về view "Index" nếu model không hợp lệ
        }
    }
}
