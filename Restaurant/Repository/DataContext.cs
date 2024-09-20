using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using System.Reflection.Metadata;

namespace Restaurant.Repository
{
    public class DataContext : DbContext         
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) {

        }
        public DbSet<UserModel> user { get; set; }
        public DbSet<CategoryModel> category { get; set; }
        public DbSet<DishModel> dish { get; set; }
        public DbSet<OrderModel> order { get; set; }
        public DbSet<OrderDetailModel> order_detail { get; set; }
        public DbSet<WebSettingModel> web_setting { get; set; }
        public DbSet<AdsModel> ads { get; set; }
        public DbSet<BlogModel> blog { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderDetailModel>()
                .HasKey(od => new { od.orderId, od.dishId });

            // Configure relationships, constraints, etc.
            modelBuilder.Entity<OrderModel>()
                .HasOne(o => o.user)
                .WithMany(u => u.order)
                .HasForeignKey(o => o.userId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DishModel>()
                .HasOne(d => d.category)
                .WithMany(c => c.dish)
                .HasForeignKey(d => d.categoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Additional configurations can be done here if necessary
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Add Your Connection String
            optionsBuilder.UseSqlServer("Data Source=LAPTOP-571RI1S9;Initial Catalog=FRest;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");
        }

    }
}
