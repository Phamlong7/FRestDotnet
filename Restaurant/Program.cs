using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Restaurant.Areas.Admin;
using Restaurant.Models;
using Restaurant.Repository;
using Restaurant.Utility;
using Restaurant.ViewModels;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Services.AddLogging();

// Configure database context
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:ConnectedDb"]);
});

// Identity configuration
builder.Services.AddIdentity<UserModel, IdentityRole<long>>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.User.AllowedUserNameCharacters = string.Empty; // Specify allowed username characters if needed
})
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();

// Add RoleManager explicitly
builder.Services.AddScoped<RoleManager<IdentityRole<long>>>();

// Add other services
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IFileService, FileService>(); // File service
builder.Services.AddSingleton<ConstantHelper>();
builder.Services.AddTransient<SendMail>();

// Configure session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true; // Prevent JavaScript access
    options.Cookie.IsEssential = true; // Ensure the cookie is set even if the user does not consent
});

// Configure authentication
builder.Services.AddAuthentication(); // Just call AddAuthentication without parameters

// Configure authentication with cookie scheme
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Redirect to login page if not authenticated
        options.LogoutPath = "/Account/Logout"; // Redirect to logout page
        options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect for access denied
    })
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    })
    .AddFacebook(facebookOptions =>
    {
        facebookOptions.AppId = builder.Configuration["Authentication:Facebook:AppId"];
        facebookOptions.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
        facebookOptions.Scope.Add("email"); // Optional: Add scope for email
    });

// Configure logging
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

// Build the app after configuring services
var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // Enable session before authentication

// Ensure authentication and authorization are enabled
app.UseAuthentication();
app.UseAuthorization();

// Configure routing for controllers and areas
app.UseEndpoints(endpoints =>
{
    // Route for Admin area
    endpoints.MapAreaControllerRoute(
        name: "Admin",
        areaName: "Admin",
        pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

    // Route for User area
    endpoints.MapAreaControllerRoute(
        name: "User",
        areaName: "User",
        pattern: "User/{controller=Home}/{action=Index}/{id?}");

    // Route for Home controller in the main application
    endpoints.MapControllerRoute(
        name: "Home",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // Default route for other controllers
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

// Map controllers
app.MapControllers();

// Run the application
app.Run();
