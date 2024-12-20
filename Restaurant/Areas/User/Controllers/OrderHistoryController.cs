﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using Restaurant.Repository;
using Restaurant.Utility;
using Restaurant.ViewModels;

namespace Restaurant.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "USER")]
    public class OrderHistoryController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<UserModel> _userManager;
        private readonly SendMail _sendMail;
        private readonly ConstantHelper _constantHelper;
        private const string CartSessionName = "CartSession";

        public OrderHistoryController(DataContext context, UserManager<UserModel> userManager, SendMail sendMail, ConstantHelper constantHelper)
        {
            _dataContext = context;
            _userManager = userManager;
            _sendMail = sendMail;
            _constantHelper = constantHelper;
        }
        public async Task<IActionResult> Index()
        {
            // Get the logged-in user's ID
            var user = await _userManager.GetUserAsync(User);
            // Fetch orders along with their related user data (or other related entities)
            var order = await _dataContext.order
                .Where(o => o.userId == user.Id) //show data with specific user
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
                var user = await _userManager.GetUserAsync(User);
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

                // Clear the cart from the session
                HttpContext.Session.Remove(CartSessionName); // Clear the cart session

                // Get the current user's email
                var currentUser = await _userManager.GetUserAsync(User);
                string userEmail = currentUser?.Email;

                // Prepare the email body using the order details
                string emailBody = GenerateEmailBody(newOrder.orderDetails); // Pass order details

                // Send confirmation email
                bool emailSent = await _sendMail.SendEmailAsync(
                    userEmail,
                    "Order Confirmation",
                    emailBody
                );

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
                .ThenInclude(od => od.dish) // Include Dish data
                .FirstOrDefault(o => o.id == id); // Find order by id

            if (order == null)
            {
                // Handle case where order is not found (e.g., redirect or show error)
                return NotFound();
            }

            return View(order); // Pass the order to the view
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

            TempData["SuccessMessage"] = "Order cancelled successfully!";
            return RedirectToAction("Index");
        }

        //generate email to send user
        private string GenerateEmailBody(IEnumerable<OrderDetailModel> orderDetails)
        {
            var body = "<h1>Your order has been successfully created!</h1><p>Thank you for your order. Here are the details:</p><table>";
            body += "<tr><th>Item</th><th>Price</th><th>Quantity</th><th>Image</th></tr>";

            foreach (var item in orderDetails)
            {
                var dish = _dataContext.dish.FirstOrDefault(d => d.id == item.dishId);

                body += $"<tr>" +
                            $"<td>{dish?.title}</td>" +
                            $"<td>{item.priceAtOrder.Value.ToString("N2")} $</td>" +
                            $"<td>{item.quantity}</td>" +
                            $"<td><img src='https://frestrestaurant-hwfdfnh9fzeke3fn.southeastasia-01.azurewebsites.net/Media/{dish?.banner}' style='width:100px;'/></td>" +//not work
                        $"</tr>";
            }

            body += "</table>";
            body += "<p>We appreciate your business!</p>";
            return body;
        }
    }
}