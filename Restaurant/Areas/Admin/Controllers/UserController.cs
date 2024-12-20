﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Models; // Include your models and data context
using Restaurant.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Restaurant.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class UserController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<UserModel> _userManager;

        // Ensure there's only one constructor
        public UserController(DataContext dataContext, UserManager<UserModel> userManager)
        {
            _dataContext = dataContext;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            // Fetch users from the AspNetUsers table with role "USER"
            var users = _dataContext.Users
                .Where(u => u.Role.ToLower() == "user") // Case-insensitive comparison
                .ToList();

            var latestComments = _dataContext.comment
                .Include(c => c.Blog)  // Ensure Blog is included
                .Where(c => c.CreatedDate >= DateTime.Now.AddDays(-7))  // Last 7 days filter
                .OrderByDescending(c => c.CreatedDate)  // Order by newest first
                .ToList();  // Retrieve all comments within the last 7 days

            var latestOrders = _dataContext.order
                .Include(o => o.orderDetails)
                .ThenInclude(od => od.dish)
                .Where(o => o.createdDate >= DateTime.Now.AddDays(-7))
                .OrderByDescending(o => o.createdDate)
                .ToList();

            var combinedNotifications = latestComments.Cast<object>()
                .Concat(latestOrders.Cast<object>())
                .OrderByDescending(n => n is CommentModel comment ? comment.CreatedDate : (n is OrderModel order ? order.createdDate : DateTime.MinValue))
                .ToList();

            ViewData["LatestComments"] = latestComments;
            ViewData["LatestOrders"] = latestOrders;
            ViewData["CombinedNotifications"] = combinedNotifications;

            // Pass the list of users to the view
            return View(users);
        }

        // GET: User/Edit/
        public async Task<IActionResult> Edit(long id)
        {
            // Fetch the user from the database by their ID
            var user = await _dataContext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user); // Pass the user model to the view
        }

        // POST: User/Edit/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, UserModel updatedUser)
        {
            // Ensure the ID in the URL matches the ID of the user being updated
            if (id != updatedUser.Id)
            {
                return BadRequest();
            }

            // Fetch the existing user from the database
            var existingUser = await _dataContext.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            // Remove validation for 'Orders' to avoid validation errors related to it
            ModelState.Remove("Orders");

            if (ModelState.IsValid)
            {
                // Manually update only the fields you need (Status, UpdatedBy, UpdatedDate)
                existingUser.Status = updatedUser.Status;
                existingUser.UpdatedBy = User.Identity.Name; // Set the updated by field to the current user's name
                existingUser.UpdatedDate = DateTime.Now;

                // Save the changes to the database
                await _dataContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "User status updated successfully!";
                return RedirectToAction("Index");
            }

            return View(updatedUser);
        }
    }
}
