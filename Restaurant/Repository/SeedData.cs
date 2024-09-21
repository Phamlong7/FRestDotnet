using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using System.Linq;
using System.Reflection;

namespace Restaurant.Repository
{
    public class SeedData
    {
        public static void SeedingData(DataContext _context)
        {
            // Apply any pending migrations
            _context.Database.Migrate();

            // Seed categories if none exist
            if (!_context.category.Any())
            {
                CategoryModel Breakfast = new CategoryModel { name = "Breakfast", description = "Hearty and delicious main courses", createdBy = "admin", updatedBy = "admin", status = "ACTIVE" };
                CategoryModel Lunch = new CategoryModel { name = "Lunch", description = "Hearty and delicious main courses", createdBy = "admin", updatedBy = "admin", status = "ACTIVE" };
                CategoryModel Dinner = new CategoryModel { name = "Dinner", description = "Delightful and savory meals for the evening.", createdBy = "admin", updatedBy = "admin", status = "ACTIVE" };
                CategoryModel Dessert = new CategoryModel { name = "Dessert", description = "Sweet treats to end your meal on a high note.", createdBy = "admin", updatedBy = "admin", status = "ACTIVE" };
                CategoryModel Drinks = new CategoryModel { name = "Drinks", description = "Refreshing beverages to complement any meal.", createdBy = "admin", updatedBy = "admin", status = "ACTIVE" };

                _context.dish.AddRange(

                    new DishModel { title = "Caesar Salad", content = "Fresh romaine lettuce, parmesan, and Caesar dressing", status = "ACTIVE", banner = "/images/caesar_salad.jpg", category = Breakfast, createdBy = "admin", updatedBy = "admin", price = 100m },
                    new DishModel { title = "BanhMi Cha", content = "Vietnamese sandwich with pork and fresh herbs", status = "ACTIVE", banner = "/images/banhmi_cha.jpg", category = Lunch, createdBy = "admin", updatedBy = "admin", price = 120m },
                    new DishModel { title = "Pancakes", content = "Fluffy pancakes served with maple syrup and butter", status = "ACTIVE", banner = "/images/pancakes.jpg", category = Breakfast, createdBy = "admin", updatedBy = "admin", price = 80m },
                    new DishModel { title = "French Toast", content = "Egg-soaked bread slices, fried and served with cinnamon sugar", status = "ACTIVE", banner = "/images/french_toast.jpg", category = Breakfast, createdBy = "admin", updatedBy = "admin", price = 90m },
                    new DishModel { title = "Club Sandwich", content = "Turkey, bacon, lettuce, tomato, and mayonnaise on toast", status = "ACTIVE", banner = "/images/club_sandwich.jpg", category = Lunch, createdBy = "admin", updatedBy = "admin", price = 130m },
                    new DishModel { title = "Grilled Cheese", content = "Melted cheese between crispy slices of bread", status = "ACTIVE", banner = "/images/grilled_cheese.jpg", category = Lunch, createdBy = "admin", updatedBy = "admin", price = 90m },
                    new DishModel { title = "Spaghetti Bolognese", content = "Traditional Italian pasta with a rich meat sauce", status = "ACTIVE", banner = "/images/spaghetti_bolognese.jpg", category = Dinner, createdBy = "admin", updatedBy = "admin", price = 150m },
                    new DishModel { title = "Grilled Salmon", content = "Salmon fillet grilled to perfection with a lemon butter sauce", status = "ACTIVE", banner = "/images/grilled_salmon.jpg", category = Dinner, createdBy = "admin", updatedBy = "admin", price = 200m },
                    new DishModel { title = "Chocolate Cake", content = "Rich chocolate cake with a creamy chocolate ganache", status = "ACTIVE", banner = "/images/chocolate_cake.jpg", category = Dessert, createdBy = "admin", updatedBy = "admin", price = 75m },
                    new DishModel { title = "Apple Pie", content = "Classic apple pie with a flaky crust and cinnamon filling", status = "ACTIVE", banner = "/images/apple_pie.jpg", category = Dessert, createdBy = "admin", updatedBy = "admin", price = 85m },
                    new DishModel { title = "Latte", content = "Creamy espresso with steamed milk", status = "ACTIVE", banner = "/images/latte.jpg", category = Drinks, createdBy = "admin", updatedBy = "admin", price = 60m },
                    new DishModel { title = "Mojito", content = "Classic refreshing cocktail made with rum, mint, and lime", status = "ACTIVE", banner = "/images/mojito.jpg", category = Drinks, createdBy = "admin", updatedBy = "admin", price = 70m }

                );
            }
            if (!_context.user.Any())
            {
                // Seed users
                UserModel admin = new UserModel { username = "admin", password = "admin", role = "ADMIN", email = "admin@gmail.com", createdBy = "admin", updatedBy = "admin" };
                UserModel user2 = new UserModel { username = "user2", password = "pass2", email = "user2@example.com", createdBy = "admin", updatedBy = "admin" };
                UserModel user3 = new UserModel { username = "user3", password = "pass3", email = "user3@example.com", createdBy = "admin", updatedBy = "admin" };
                UserModel user4 = new UserModel { username = "user4", password = "pass4", email = "user4@example.com", createdBy = "admin", updatedBy = "admin" };
                UserModel user5 = new UserModel { username = "user5", password = "pass5", email = "user5@example.com", createdBy = "admin", updatedBy = "admin" };
                UserModel user6 = new UserModel { username = "user6", password = "pass6", email = "user6@example.com", createdBy = "admin", updatedBy = "admin" };
                UserModel user7 = new UserModel { username = "user7", password = "pass7", email = "user7@example.com", createdBy = "admin", updatedBy = "admin" };
                UserModel user8 = new UserModel { username = "user8", password = "pass8", email = "user8@example.com", createdBy = "admin", updatedBy = "admin" };
                UserModel user9 = new UserModel { username = "user9", password = "pass9", email = "user9@example.com", createdBy = "admin", updatedBy = "admin" };
                UserModel user10 = new UserModel { username = "user10", password = "pass10", email = "user10@example.com", createdBy = "admin" };

                _context.user.AddRange(admin, user2, user3, user4, user5, user6, user7, user8, user9, user10);
                _context.SaveChanges();

                // Seed orders
                _context.order.AddRange(
                    new OrderModel { message = "Order 1", total = 500, createdBy = "user1", updatedBy = "admin", userId = 1 },
                    new OrderModel { message = "Order 2", total = 600, createdBy = "user2", updatedBy = "admin", userId = 2 },
                    new OrderModel { message = "Order 3", total = 700, createdBy = "user3", updatedBy = "admin", userId = 3 },
                    new OrderModel { message = "Order 4", total = 800, createdBy = "user4", updatedBy = "admin", userId = 4 },
                    new OrderModel { message = "Order 5", total = 900, createdBy = "user5", updatedBy = "admin", userId = 5 },
                    new OrderModel { message = "Order 6", total = 1000, createdBy = "user6", updatedBy = "admin", userId = 6 },
                    new OrderModel { message = "Order 7", total = 1100, createdBy = "user7", updatedBy = "admin", userId = 7 },
                    new OrderModel { message = "Order 8", total = 1200, createdBy = "user8", updatedBy = "admin", userId = 8 },
                    new OrderModel { message = "Order 9", total = 1300, createdBy = "user9", updatedBy = "admin", userId = 9 },
                    new OrderModel { message = "Order 10", total = 1400, createdBy = "user10", updatedBy = "admin", userId = 10 }
                );
                _context.SaveChanges();

                // Seed order details
                _context.order_detail.AddRange(
                    new OrderDetailModel { orderId = 1, dishId = 1, quantity = 2 },
                    new OrderDetailModel { orderId = 2, dishId = 2, quantity = 1 },
                    new OrderDetailModel { orderId = 3, dishId = 3, quantity = 4 },
                    new OrderDetailModel { orderId = 4, dishId = 4, quantity = 2 },
                    new OrderDetailModel { orderId = 5, dishId = 5, quantity = 3 },
                    new OrderDetailModel { orderId = 6, dishId = 6, quantity = 5 },
                    new OrderDetailModel { orderId = 7, dishId = 7, quantity = 2 },
                    new OrderDetailModel { orderId = 8, dishId = 8, quantity = 1 },
                    new OrderDetailModel { orderId = 9, dishId = 9, quantity = 4 },
                    new OrderDetailModel { orderId = 10, dishId = 10, quantity = 3 }
                );
            }

            if (!_context.blog.Any())
                {
                _context.blog.AddRange(
                    new BlogModel { title = "Blog Post 1", content = "Content of blog 1", createdBy = "admin", updatedBy = "admin", banner = "image1.jpg" },
                    new BlogModel { title = "Blog Post 2", content = "Content of blog 2", createdBy = "admin", updatedBy = "admin", banner = "image2.jpg" },
                    new BlogModel { title = "Blog Post 3", content = "Content of blog 3", createdBy = "admin", updatedBy = "admin", banner = "image5.jpg" },
                    new BlogModel { title = "Blog Post 4", content = "Content of blog 4", createdBy = "admin", updatedBy = "admin", banner = "image2.jpg" },
                    new BlogModel { title = "Blog Post 5", content = "Content of blog 5", createdBy = "admin", updatedBy = "admin", banner = "image4.jpg" },
                    new BlogModel { title = "Blog Post 6", content = "Content of blog 6", createdBy = "admin", updatedBy = "admin", banner = "image1.jpg" },
                    new BlogModel { title = "Blog Post 7", content = "Content of blog 7", createdBy = "admin", updatedBy = "admin", banner = "image7.jpg" },
                    new BlogModel { title = "Blog Post 8", content = "Content of blog 8", createdBy = "admin", updatedBy = "admin", banner = "image6.jpg" },
                    new BlogModel { title = "Blog Post 9", content = "Content of blog 9", createdBy = "admin", updatedBy = "admin", banner = "image8.jpg" },
                    new BlogModel { title = "Blog Post 10", content = "Content of blog 10", createdBy = "admin" , banner = "image8.jpg" }
                );
                }
                if (!_context.ads.Any())
                {
                _context.ads.AddRange(
                     new AdsModel { images = "/ads/ad1.jpg", url = "http://ad1.com", createdBy = "admin", updatedBy = "admin",width = 200, height = 100, position = "middle" },
                     new AdsModel { images = "/ads/ad2.jpg", url = "http://ad2.com", createdBy = "admin", updatedBy = "admin", width = 300, height = 300, position = "middle" },
                     new AdsModel { images = "/ads/ad3.jpg", url = "http://ad3.com", createdBy = "admin", updatedBy = "admin", width = 400, height = 200, position = "middle" },
                     new AdsModel {images = "/ads/ad4.jpg", url = "http://ad4.com", createdBy = "admin", updatedBy = "admin", width = 720, height = 100, position = "middle" },
                     new AdsModel {images = "/ads/ad5.jpg", url = "http://ad5.com", createdBy = "admin", updatedBy = "admin", width = 250, height = 160, position = "left" },
                     new AdsModel {images = "/ads/ad6.jpg", url = "http://ad6.com", createdBy = "admin", updatedBy = "admin", width = 270, height = 150, position = "middle" },
                     new AdsModel {images = "/ads/ad7.jpg", url = "http://ad7.com", createdBy = "admin", updatedBy = "admin", width = 880, height = 280, position = "middle" },
                     new AdsModel {images = "/ads/ad8.jpg", url = "http://ad8.com", createdBy = "admin", updatedBy = "admin", width = 730, height = 390, position = "down" },
                     new AdsModel {images = "/ads/ad9.jpg", url = "http://ad9.com", createdBy = "admin", updatedBy = "admin", width = 380, height = 400, position = "above" },
                     new AdsModel {images = "/ads/ad10.jpg", url = "http://ad10.com", createdBy = "admin", updatedBy = "admin", width = 200, height = 1400, position = "top" }
                 );
                }
                if (!_context.web_setting.Any())
                {
                _context.web_setting.AddRange(
                       new WebSettingModel { content = "Homepage Banner", createdBy = "admin", updatedBy = "admin", type = "BANNER", image = "banner1.jpg", status = "ACTIVE" },
                       new WebSettingModel { content = "Footer Text", createdBy = "admin", updatedBy = "admin", type = "FOOTER", image = "banner2.jpg", status = "ACTIVE" },
                       new WebSettingModel { content = "About Us Section", createdBy = "admin", updatedBy = "admin", type = "ABOUT_US", image = "about_us.jpg", status = "ACTIVE" },
                       new WebSettingModel { content = "Contact Information", createdBy = "admin", updatedBy = "admin", type = "CONTACT", image = "contact_info.jpg", status = "ACTIVE" }
                   );
            }    

                _context.SaveChanges();
        }
    }
}
