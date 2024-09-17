using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace Restaurant.Repository
{
    public class SeedData
    {
        public static void SeedingData(DataContext _context)
        {
            _context.Database.Migrate();
            if (!_context.Dish.Any()) { 
                DishModel Grilled = new DishModel { Title= "Grilled Beef with potatoes", Content= "Meat, Potatoes, Rice, Tomatoe", Status= "1",Banner= "1.jpg"};
            }
        }
    }
}
