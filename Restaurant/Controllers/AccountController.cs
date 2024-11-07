using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Restaurant.Models;
using Restaurant.Utility;
using Restaurant.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Restaurant.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<UserModel> _signInManager;
        private readonly UserManager<UserModel> _userManager;
        private readonly RoleManager<IdentityRole<long>> _roleManager;
        private readonly ConstantHelper _constantHelper;
        private readonly SendMail _sendMail;
        private readonly ILogger<AccountController> _logger;
        private const string CartSessionName = "CartSession";

        public AccountController(
             SignInManager<UserModel> signInManager,
             UserManager<UserModel> userManager,
             RoleManager<IdentityRole<long>> roleManager,
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
                UserModel? user;

                // Determine if input is email or username
                if (new EmailAddressAttribute().IsValid(model.EmailOrUsername))
                {
                    user = await _userManager.FindByEmailAsync(model.EmailOrUsername);
                }
                else
                {
                    user = await _userManager.FindByNameAsync(model.EmailOrUsername);
                }

                if (user == null)
                {
                    ModelState.AddModelError("", "User not found.");
                    return View(model);
                }

                if (!user.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("", "Your account has been banned or deactivated.");
                    return View(model);
                }

                // Sign in the user
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    // Fetch roles using UserManager
                    var roles = await _userManager.GetRolesAsync(user);
                    string userRole = roles.FirstOrDefault() ?? "USER"; // Default to "USER" if no roles are assigned

                    // Ensure role claim is added for the session
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, userRole) // Role claim from UserManager
            };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    TempData["SuccessMessage"] = $"Login successful! Welcome {user.UserName}";

                    // Role-based redirection
                    if (userRole.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                    else if (userRole.Equals("USER", StringComparison.OrdinalIgnoreCase))
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
                // Check if the email is already in use
                var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingUserByEmail != null)
                {
                    ModelState.AddModelError("", "Email is already in use.");
                    return View(model);
                }

                // Create a temporary user object without saving to the database
                UserModel user = new UserModel
                {
                    Email = model.Email,
                    UserName = model.Name,
                    Role = "USER",
                    CreatedDate = DateTime.Now,
                    CreatedBy = "USER",
                    Status = "ACTIVE"
                };

                // Generate OTP
                string otp = _constantHelper.GenerateOTP();

                // Store user data, including the password and OTP temporarily
                TempData["UserData"] = JsonConvert.SerializeObject(user);
                TempData["UserPassword"] = model.Password; // Store password temporarily
                TempData["OTP"] = otp;

                // Send OTP via email
                string subject = "Verify Your Email";
                string body = $"Your OTP is: {otp}. It will expire in 5 minutes.";
                bool emailSent = await _sendMail.SendEmailAsync(user.Email, subject, body);

                if (emailSent)
                {
                    TempData["SuccessMessage"] = "Registration successful! Please check your email to verify your account.";
                    return RedirectToAction("VerifyOTP", "Account");
                }
                else
                {
                    ModelState.AddModelError("", "Failed to send verification email. Please try again.");
                }
            }

            return View(model);
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
                string? userDataJson = TempData["UserData"] as string;
                string? userPassword = TempData["UserPassword"] as string; // Retrieve the stored password

                if (string.IsNullOrEmpty(storedOTP) || string.IsNullOrEmpty(userDataJson) || string.IsNullOrEmpty(userPassword))
                {
                    ModelState.AddModelError("", "OTP has expired. Please request a new one.");
                    return View();
                }

                if (otp == storedOTP)
                {
                    // Deserialize user data
                    UserModel? user = JsonConvert.DeserializeObject<UserModel>(userDataJson);
                    if (user == null)
                    {
                        ModelState.AddModelError("", "Failed to deserialize user data.");
                        return View();
                    }
                    // Create the user in the database using the stored password
                    var result = await _userManager.CreateAsync(user, userPassword);
                    if (result.Succeeded)
                    {
                        // Assign the USER role
                        if (!await _roleManager.RoleExistsAsync("USER"))
                        {
                            await _roleManager.CreateAsync(new IdentityRole<long>("USER"));
                        }
                        await _userManager.AddToRoleAsync(user, "USER");

                        // Mark the user's email as confirmed
                        user.EmailConfirmed = true;
                        await _userManager.UpdateAsync(user);

                        TempData["SuccessMessage"] = "Your email has been successfully verified! You can now log in.";
                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid OTP. Please try again.");
                }
            }

            return View();
        }

        [HttpGet]
        public IActionResult VerifyOTPForExternal()
        {
            return View(_constantHelper);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOTPForExternal(string otp)
        {
            // Ensure that ModelState is valid before proceeding.
            if (ModelState.IsValid)
            {
                string? storedOTP = TempData["OTP"] as string;
                string? userDataJson = TempData["UserData"] as string;

                // Check if the OTP or user data is missing.
                if (string.IsNullOrEmpty(storedOTP) || string.IsNullOrEmpty(userDataJson))
                {
                    ModelState.AddModelError("", "OTP has expired. Please request a new one.");
                    return View();
                }

                // Check if the provided OTP matches the stored OTP.
                if (otp == storedOTP)
                {
                    // Deserialize the user data.
                    UserModel? user = JsonConvert.DeserializeObject<UserModel>(userDataJson);
                    if (user == null || string.IsNullOrWhiteSpace(user.UserName) || string.IsNullOrWhiteSpace(user.Email))
                    {
                        ModelState.AddModelError("", "Failed to deserialize user data or user data is incomplete.");
                        return View();
                    }

                    // Create the user in the database.
                    var createResult = await _userManager.CreateAsync(user);
                    if (createResult.Succeeded)
                    {
                        // Assign the USER role if it doesn't already exist.
                        if (!await _roleManager.RoleExistsAsync("USER"))
                        {
                            await _roleManager.CreateAsync(new IdentityRole<long>("USER"));
                        }
                        await _userManager.AddToRoleAsync(user, "USER");

                        // Mark the user's email as confirmed.
                        user.EmailConfirmed = true;
                        await _userManager.UpdateAsync(user);

                        // Sign in the user.
                        await _signInManager.SignInAsync(user, isPersistent: false); // No persistent login

                        TempData["SuccessMessage"] = "Your account has been successfully created! You are now logged in.";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        // Handle any errors that occurred during user creation.
                        foreach (var error in createResult.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid OTP. Please try again.");
                }
            }

            // Return to the view with the current model state if validation fails.
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
            var passwordVerificationResult = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash ?? string.Empty, model.NewPassword);
            if (passwordVerificationResult == PasswordVerificationResult.Success)
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
            // Clear the cart from the session
            HttpContext.Session.Remove(CartSessionName); // Clear the cart session
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme); 
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            // Request a redirect to the external login provider
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            // Default return URL
            returnUrl ??= Url.Content("~/");

            // Check if there is an error from the remote provider
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return RedirectToAction(nameof(Login)); // Redirect to login page if there's an error
            }

            try
            {
                // Get info from the external provider
                var info = await _signInManager.GetExternalLoginInfoAsync();

                // Handle the case where the user cancels the external login
                if (info == null)
                {
                    // User has canceled the external login process
                    ModelState.AddModelError(string.Empty, "External login was canceled.");
                    return RedirectToAction(nameof(Login)); // Redirect back to login page
                }

                // Attempt to sign in with the external login
                var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

                // Check if sign in was successful
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                    if (user != null)
                    {
                        // Check if the user is ACTIVE
                        if (!user.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase))
                        {
                            await _signInManager.SignOutAsync(); // Ensure the user is signed out
                            ModelState.AddModelError(string.Empty, "Your account has been banned or deactivated.");
                            return RedirectToAction(nameof(Login)); // Redirect back to login with error
                        }
                    }

                    _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal?.Identity?.Name, info.LoginProvider);
                    return LocalRedirect(returnUrl); // Redirect on success
                }
                else if (result.IsLockedOut)
                {
                    return RedirectToAction("Lockout"); // Handle locked-out users
                }
                else
                {
                    // If user does not have an account, ask them to create one
                    var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                    if (email != null)
                    {
                        // Check if the user already exists
                        var user = await _userManager.FindByEmailAsync(email);
                        if (user == null)
                        {
                            // Redirect to the registration view to collect username and email
                            var model = new ExternalLoginRegistrationViewModel
                            {
                                Email = email // Pre-fill email from external provider
                            };
                            ViewData["ReturnUrl"] = returnUrl; // Store return URL for later
                            return View("ExternalLoginRegistration", model); // Show registration view
                        }
                        else
                        {
                            // Check if the user is ACTIVE before linking the account
                            if (!user.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase))
                            {
                                ModelState.AddModelError(string.Empty, "Your account has been banned or deactivated.");
                                return RedirectToAction(nameof(Login)); // Redirect back to login with error
                            }

                            // Link the external login to their existing account
                            await _userManager.AddLoginAsync(user, info);
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl); // Redirect after linking
                        }
                    }

                    // If email was not provided by the external provider
                    ModelState.AddModelError(string.Empty, "Email claim not received from provider.");
                    return RedirectToAction(nameof(Login));
                }
            }
            catch (AuthenticationFailureException ex)
            {
                _logger.LogError(ex, "An error occurred during external authentication.");
                ModelState.AddModelError(string.Empty, "Authentication failed. Please try again.");
                return RedirectToAction(nameof(Login)); // Redirect back to login page with error
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during external authentication.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
                return RedirectToAction(nameof(Login)); // Redirect back to login page with error
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginRegistration(ExternalLoginRegistrationViewModel model, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Create a temporary user object without saving to the database
                var user = new UserModel
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    CreatedDate = DateTime.Now,
                };

                // Generate OTP
                string otp = _constantHelper.GenerateOTP();

                // Store user data and OTP temporarily
                TempData["UserData"] = JsonConvert.SerializeObject(user);
                TempData["OTP"] = otp;

                // Send OTP via email
                string subject = "Verify Your Email";
                string body = $"Your OTP is: {otp}. It will expire in 5 minutes.";
                bool emailSent = await _sendMail.SendEmailAsync(user.Email, subject, body);

                if (emailSent)
                {
                    TempData["SuccessMessage"] = "Registration successful! Please check your email to verify your account.";
                    return RedirectToAction("VerifyOTPForExternal", "Account");
                }
                else
                {
                    ModelState.AddModelError("", "Failed to send verification email. Please try again.");
                }
            }

            // If we got this far, something failed, redisplay form
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

    }

}