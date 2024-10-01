using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Models;
using Restaurant.ViewModels;
using System.Security.Claims;

namespace Restaurant.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<UserModel> _signInManager;
        private readonly Microsoft.AspNetCore.Identity.UserManager<UserModel> _userManager;
        private readonly Microsoft.AspNetCore.Identity.RoleManager<IdentityRole<long>> _roleManager;
        private readonly ConstantHelper _constantHelper;
        private readonly SendMail _sendMail;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
             SignInManager<UserModel> signInManager,
             Microsoft.AspNetCore.Identity.UserManager<UserModel> userManager,
             Microsoft.AspNetCore.Identity.RoleManager<IdentityRole<long>> roleManager,
             ILogger<AccountController> logger,
             ConstantHelper constantHelper,
             SendMail sendMail)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _constantHelper = constantHelper;
            _sendMail = sendMail;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "User not found.");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    // Add role claim
                    var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Role, user.Role)
                        };

                    await _userManager.AddClaimsAsync(user, claims);

                    // Sign in again to refresh the cookie with the new claims
                    await _signInManager.RefreshSignInAsync(user);

                    // Set success notification
                    TempData["SuccessMessage"] = "Login successful! Welcome " + user.UserName;

                    if (user.Role.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else if (user.Role.Equals("USER", StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "User role is not recognized.");
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
                }
            }

            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                UserModel user = new UserModel
                {
                    Email = model.Email,
                    UserName = model.Name,
                    EmailConfirmed = false // Make sure the email is not confirmed yet
                };

                // Create the user
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Check if the USER role exists, create it if it doesn't
                    if (!await _roleManager.RoleExistsAsync("USER"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole<long>("USER"));
                    }

                    // Assign the USER role
                    var roleResult = await _userManager.AddToRoleAsync(user, "USER");

                    if (roleResult.Succeeded)
                    {
                        // Generate OTP
                        string otp = _constantHelper.GenerateOTP();

                        // Store OTP and email temporarily (can also store in a more persistent way)
                        TempData["OTP"] = otp;
                        TempData["UserEmail"] = user.Email;

                        // Send OTP via email
                        string subject = "Verify Your Email";
                        string body = $"Your OTP is: {otp}. It will expire in 10 minutes.";

                        bool emailSent = await _sendMail.SendEmailAsync(user.Email, subject, body);

                        if (emailSent)
                        {
                            // Redirect to OTP verification page
                            TempData["SuccessMessage"] = "Registration successful! Please check your email to verify your account.";
                            return RedirectToAction("VerifyOTP", "Account");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Failed to send verification email. Please try again.");
                        }
                    }
                    else
                    {
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View(model);
                    }
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
            }

            return View(model);
        }

        public IActionResult VerifyEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "User not found!");
                    return View(model);
                }

                string otp = _constantHelper.GenerateOTP();

                // Store OTP (consider using a more secure method in production)
                TempData["OTP"] = otp;
                TempData["UserEmail"] = model.Email;

                string subject = "Your OTP for Register";
                string body = $"Your OTP is: {otp}. It will expire in 10 seconds.";

                bool emailSent = await _sendMail.SendEmailAsync(model.Email, subject, body);

                if (!emailSent)
                {
                    ModelState.AddModelError("", "Failed to send OTP. Please try again.");
                    return View(model);
                }

                return RedirectToAction("VerifyOTP", "Account");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while verifying email for user {Email}", model.Email);
                ModelState.AddModelError("", "An error occurred. Please try again later.");
                return View(model);
            }
        }
        public IActionResult VerifyEmailForChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmailForChangePassword(VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "User not found!");
                    return View(model);
                }

                string otp = _constantHelper.GenerateOTP();

                // Store OTP (consider using a more secure method in production)
                TempData["OTP"] = otp;
                TempData["UserEmail"] = model.Email;

                string subject = "Your OTP for Password Reset";
                string body = $"Your OTP is: {otp}. It will expire in 10 seconds.";

                bool emailSent = await _sendMail.SendEmailAsync(model.Email, subject, body);

                if (!emailSent)
                {
                    ModelState.AddModelError("", "Failed to send OTP. Please try again.");
                    return View(model);
                }

                return RedirectToAction("VerifyOTPForChangePassword", "Account");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while verifying email for user {Email}", model.Email);
                ModelState.AddModelError("", "An error occurred. Please try again later.");
                return View(model);
            }
        }

        private async Task<bool> SendOTPEmail(string email, string subject, string body)
        {
            try
            {
                return await Task.Run(() => _sendMail.SendEmailAsync(email, subject, body));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP email to {Email}", email);
                return false;
            }
        }

        [HttpGet]
        public IActionResult VerifyOTP()
        {
            return View(_constantHelper);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOTP(string otp)
        {
            if (ModelState.IsValid)
            {
                string? storedOTP = TempData["OTP"] as string;
                string? userEmail = TempData["UserEmail"] as string;

                if (string.IsNullOrEmpty(storedOTP) || string.IsNullOrEmpty(userEmail))
                {
                    ModelState.AddModelError("", "OTP has expired. Please request a new one.");
                    return View();
                }

                if (otp == storedOTP)
                {
                    // Find the user by email
                    var user = await _userManager.FindByEmailAsync(userEmail);
                    if (user == null)
                    {
                        ModelState.AddModelError("", "User not found.");
                        return View();
                    }

                    // Mark the user's email as confirmed
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);

                    TempData["SuccessMessage"] = "Your email has been successfully verified! You can now log in.";
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid OTP. Please try again.");
                }
            }

            return View();
        }

        [HttpGet]
        public IActionResult VerifyOTPForChangePassword()
        {
            return View(_constantHelper);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOTPForChangePassword(string otp)
        {
            if (ModelState.IsValid)
            {
                string? storedOTP = TempData["OTP"] as string;
                string? userEmail = TempData["UserEmail"] as string;

                if (string.IsNullOrEmpty(storedOTP) || string.IsNullOrEmpty(userEmail))
                {
                    ModelState.AddModelError("", "OTP has expired. Please request a new one.");
                    ClearOTPTempData();
                    return View();
                }

                if (otp == storedOTP)
                {
                    var user = await _userManager.FindByEmailAsync(userEmail);
                    if (user == null)
                    {
                        ModelState.AddModelError("", "User not found.");
                        ClearOTPTempData();
                        return View();
                    }

                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);

                    ClearOTPTempData();
                    TempData["SuccessMessage"] = "Your email has been successfully verified! You can now change your password.";
                    return RedirectToAction("ChangePassword", "Account", new { email = userEmail });
                }
                else
                {
                    ModelState.AddModelError("", "Invalid OTP. Please try again.");
                    ClearOTPTempData();
                }
            }

            return View();
        }

        private void ClearOTPTempData()
        {
            TempData.Remove("OTP");
            TempData.Remove("UserEmail");
        }

        [HttpGet]
        public IActionResult ChangePassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Email is required to change password.";
                return RedirectToAction("Login", "Account");
            }
            return View(new ChangePasswordViewModel { Email = email });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user doesn't exist
                ModelState.AddModelError("", "Unable to change password. Please try again.");
                return View(model);
            }

            // Check if the new password is the same as the current password
            var newPasswordHash = _userManager.PasswordHasher.HashPassword(user, model.NewPassword);
            var passwordVerificationResult = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, model.NewPassword);
            if (passwordVerificationResult == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success)
            {
                ModelState.AddModelError("", "The new password must be different from the current password.");
                return View(model);
            }

            // Generate a password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Reset the password
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (result.Succeeded)
            {
                // Refresh sign-in cookie if the user is currently signed in
                if (User?.Identity?.IsAuthenticated == true)
                {
                    await _signInManager.RefreshSignInAsync(user);
                }

                TempData["SuccessMessage"] = "Your password has been changed successfully.";
                return RedirectToAction("Login", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

    }

}