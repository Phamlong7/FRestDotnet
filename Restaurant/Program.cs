using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Restaurant.Areas.Admin;
using Restaurant.Models;
using Restaurant.Repository;
using Restaurant.Utility;
using Restaurant.ViewModels;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Configure services before building the app
builder.Services.AddLogging();

// Connection Database 
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
    options.User.AllowedUserNameCharacters = string.Empty;
})
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();

// Add RoleManager explicitly
builder.Services.AddScoped<RoleManager<IdentityRole<long>>>();

// Add services to the container  
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IFileService, FileService>(); //file service
builder.Services.AddSingleton<ConstantHelper>();
builder.Services.AddTransient<SendMail>();

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    options.DefaultChallengeScheme = IdentityConstants.ExternalScheme;
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


// Configure logging before building the app
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

// Build the app after configuring services
var app = builder.Build();

// Configure the HTTP request pipeline  
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); // Ensures authentication is enabled  
app.UseAuthorization();  // Ensures authorization is enabled
app.UseSession(); // Now sessions are configured and can be used

// Route Controller  
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

    // Route for Home controller in main application
    endpoints.MapControllerRoute(
        name: "Home",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // Default route for other controllers
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

// Continue with the rest of the setup
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
