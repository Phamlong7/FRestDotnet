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
                CategoryModel Breakfast = new CategoryModel { name = "Breakfast",description = "Hearty and delicious main courses", createdBy = "admin", updatedBy = "admin", status = "ACTIVE" };
                CategoryModel Lunch = new CategoryModel { name = "Lunch", description = "Hearty and delicious main courses", createdBy = "admin", updatedBy = "admin", status = "ACTIVE" };

                _context.dish.AddRange(

                    new DishModel { title = "Caesar Salad", content = "Fresh romaine lettuce, parmesan, and Caesar dressing", status = "ACTIVE", banner = "/images/caesar_salad.jpg", category = Breakfast, createdBy ="admin",updatedBy ="admin", price = 100m },
                    new DishModel { title = "BanhMi Cha", content = "Fresh romaine lettuce, parmesan, and Caesar dressing", status = "ACTIVE", banner = "/images/caesar_salad.jpg", category = Lunch, createdBy ="admin", updatedBy ="admin", price = 100m }
                );
                _context.SaveChanges();
            }
        }
    }
}
