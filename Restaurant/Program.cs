using Restaurant.Repository;
using Microsoft.AspNetCore.Identity;
using Restaurant.Models;
using Microsoft.EntityFrameworkCore;
using Restaurant.ViewModels;

var builder = WebApplication.CreateBuilder(args);

// Configure services before building the app

// Connection Database 
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:ConnectedDb"]);
});

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
})
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();

// Add RoleManager explicitly
builder.Services.AddScoped<RoleManager<IdentityRole<long>>>();

// Add services to the container  
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ConstantHelper>();
builder.Services.AddTransient<SendMail>();

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

// Route Controller  
#pragma warning disable
app.UseEndpoints(endpoints =>
{
    endpoints.MapAreaControllerRoute(
        name: "Admin",
        areaName: "Admin",
        pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

// Continue with the rest of the setup
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
