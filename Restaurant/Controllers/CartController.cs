﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Repository;
using Restaurant.Utility;
using Restaurant.ViewModels;

namespace Restaurant.Controllers
{
    [Authorize(Roles = "USER, ADMIN")]
    public class CartController : Controller
    {
        private readonly DataContext _dataContext;
        private const string CartSessionName = "CartSession";

        public CartController(DataContext context)
        {
            _dataContext = context;
        }

        public IActionResult Index()
        {
            // Retrieve the cart items from the session
            var carts = HttpContext.Session.Get<List<CartItemViewModel>>(CartSessionName) ?? new List<CartItemViewModel>();

            // Prepare a list to hold the CartItemViewModels
            var cartItems = new List<CartItemViewModel>();

            ViewData["NumberCart"] = carts.Count;

            foreach (var cart in carts)
            {
                // Retrieve the dish details from your database using the cart.DishId
                var dish = _dataContext.dish.Find(cart.DishId);

                if (dish != null)
                {
                    cartItems.Add(new CartItemViewModel
                    {
                        DishId = cart.DishId,
                        Title = dish.title,
                        Price = dish.price,
                        Quantity = cart.Quantity,
                        Banner = dish.banner
                    });
                }
            }
            return View(cartItems);
        }

        [HttpPost]
        public IActionResult Add(CartItemViewModel cartItem)
        {
            try
            {
                var carts = HttpContext.Session.Get<List<CartItemViewModel>>(CartSessionName) ?? new List<CartItemViewModel>();

                var cartExist = carts.FirstOrDefault(x => x.DishId == cartItem.DishId);
                if (cartExist is null)
                {
                    carts.Add(cartItem); // Add new dish to cart
                }
                else
                {
                    cartExist.Quantity += cartItem.Quantity; // Update quantity if already in cart
                }

                HttpContext.Session.Set(CartSessionName, carts); // Save updated cart in session                                      

                return Json(carts.Count); // Return the total number of items in the cart
            }
            catch (Exception)
            {
                return Json(-1); // Return -1 on error
            }
        }

        [HttpPost]
        public IActionResult UpdateQuantity(long dishId, int newQuantity)
        {
            try
            {
                var carts = HttpContext.Session.Get<List<CartItemViewModel>>(CartSessionName) ?? new List<CartItemViewModel>();

                var cartItem = carts.FirstOrDefault(x => x.DishId == dishId);
                if (cartItem is not null)
                {
                    cartItem.Quantity = newQuantity; // Update the quantity
                    HttpContext.Session.Set(CartSessionName, carts); // Save updated cart in session
                }

                // Return success with updated cart info (could be grand total, or number of items, etc.)
                return Json(new { success = true, newQuantity = cartItem.Quantity });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public IActionResult Remove(long dishId)
        {
            var carts = HttpContext.Session.Get<List<CartItemViewModel>>(CartSessionName) ?? new List<CartItemViewModel>();
            var cartItem = carts.FirstOrDefault(x => x.DishId == dishId);

            if (cartItem != null)
            {
                carts.Remove(cartItem); // Remove item from cart
                bool isEmpty = !carts.Any(); // Check if the cart is empty

                HttpContext.Session.Set(CartSessionName, carts); // Save updated cart in session

                return Json(new { success = true, numberCart = carts.Count, isEmpty }); // Return empty flag
            }

            return Json(new { success = false }); // Item not found
        }

    }
}