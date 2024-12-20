﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using Restaurant.Repository;
using Restaurant.ViewModels;

namespace Restaurant.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class OrderController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<UserModel> _userManager;

        public OrderController(DataContext context, UserManager<UserModel> userManager)
        {
            _dataContext = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
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

            // Fetch orders along with their related user data (or other related entities)
            var order = await _dataContext.order
                //.Include(o => o.user) // Assuming you want to include user info
                .OrderBy(o => o.id)
                .ToListAsync();

            return View(order); // Returning orders to the view instead of 'dishes'
        }

        // GET: Order/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderViewModel model)
        {
            // Check if any dishes are added
            if (model.OrderDetails == null || !model.OrderDetails.Any())
            {
                TempData["ErrorMessage"] = "You must add at least one dish to the order.";
                return View(model);
            }

            if (ModelState.IsValid)
            {
                // Get the current user's ID
                var user = await _userManager.GetUserAsync(User); // Or you can use User.Identity.Name depending on your setup

                var orderDetails = model.OrderDetails.Select(od => new OrderDetailModel
                {
                    dishId = od.DishId,
                    quantity = od.Quantity,
                    priceAtOrder = od.Price
                }).ToList();

                // Sum priceAtOrder * quantity for the total
                var total = orderDetails.Sum(od => (od.priceAtOrder ?? 0) * od.quantity);

                var newOrder = new OrderModel
                {
                    message = model.Message,
                    userId = user.Id, // Set the user ID here
                    total = total,
                    createdBy = user.UserName,
                    orderDetails = orderDetails
                };

                _dataContext.order.Add(newOrder);
                await _dataContext.SaveChangesAsync(); // Use async save

                TempData["SuccessMessage"] = "Order created successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                // Debugging information
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }

            return View(model);
        }

        // Action to show order details
        public IActionResult Detail(long id)
        {
            // Retrieve the order along with order details
            var order = _dataContext.order
                .Include(o => o.user) // include relative user
                .Include(o => o.orderDetails) // Include order details
                .ThenInclude(od => od.dish) // Include Dish data if you have a navigation property
                .FirstOrDefault(o => o.id == id); // Find order by id

            if (order == null)
            {
                // Handle case where order is not found (e.g., redirect or show error)
                return NotFound();
            }

            return View(order); // Pass the order to the view
        }
        // GET: Order/Edit/
        public IActionResult Edit(long id)
        {
            // Fetch the order with related details
            var order = _dataContext.order
                .Include(o => o.orderDetails)
                .ThenInclude(od => od.dish)
                .FirstOrDefault(o => o.id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Retrieve the user information based on the userId from the order
            var user = _userManager.Users.FirstOrDefault(u => u.Id == order.userId);
            if (user != null)
            {
                ViewBag.UserName = user.UserName; // Pass the username to the view
            }

            // Map OrderModel to OrderViewModel
            var viewModel = new OrderViewModel
            {
                OrderId = order.id,
                UserId = order.userId, // Adjust based on your OrderModel structure
                CreatedDate = order.createdDate,
                Total = order.total,
                Status = order.status,
                Message = order.message,
                OrderDetails = order.orderDetails.Select(od => new OrderDetailViewModel
                {
                    DishName = od.dish.title, // Ensure 'title' is the correct property name
                    Price = od.priceAtOrder,
                    Quantity = od.quantity,
                    Total = (od.priceAtOrder ?? 0) * od.quantity // Calculate total based on quantity and priceAtOrder
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Order/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OrderViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Fetch the existing order from the database
                var order = await _dataContext.order
                    .Include(o => o.orderDetails) // Include order details if you want to modify them
                    .FirstOrDefaultAsync(o => o.id == viewModel.OrderId);

                if (order != null)
                {
                    // Update the order's properties
                    order.status = viewModel.Status;
                    order.updatedBy = (await _userManager.GetUserAsync(User)).UserName;
                    order.updatedDate = DateTime.Now;

                    // Save changes to the database
                    await _dataContext.SaveChangesAsync();
                    // Set success message in TempData
                    TempData["SuccessMessage"] = "Order edited successfully!";
                    return RedirectToAction("Index");
                }
            }
            return View(viewModel); // Return view with the model to show validation errors
        }




        public async Task<IActionResult> Delete(long id)
        {
            var order = await _dataContext.order
                .Include(o => o.orderDetails) // Include order details for deletion
                .FirstOrDefaultAsync(o => o.id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Remove order details
            _dataContext.orderDetails.RemoveRange(order.orderDetails);

            // Remove the order
            _dataContext.order.Remove(order);

            await _dataContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Order deleted successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult SearchDishes(string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return Json(new List<object>()); // Return an empty JSON array if the term is empty
            }

            var dishes = _dataContext.dish
                .Where(d => d.title.Contains(term))
                .Select(d => new { d.title, d.price, d.id })
                .ToList();

            return Json(dishes);
        }

    }
}