using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace Restaurant.Repository
{
    public class DataContext : DbContext         
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { 

        }
        public DbSet<AdsModel> Adds { get; set; }
        public DbSet<BlogModel> Blogs { get; set; }
        public DbSet<CategoryModel> Category { get; set; }
        public DbSet<DishModel> Dish { get; set; }
        public DbSet<OrderModel> Order { get; set; }
        public DbSet<UserModel> User { get; set; }

    }
}
