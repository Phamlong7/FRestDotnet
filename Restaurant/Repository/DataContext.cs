using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace Restaurant.Repository
{
    public class DataContext : IdentityDbContext<UserModel, IdentityRole<long>, long>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<CategoryModel> category { get; set; }
        public DbSet<DishModel> dish { get; set; }
        public DbSet<OrderModel> order { get; set; }
        public DbSet<OrderDetailModel> orderDetails { get; set; }
        public DbSet<WebSettingModel> web_setting { get; set; }
        public DbSet<AdsModel> ads { get; set; }
        public DbSet<BlogModel> blog { get; set; }
        public DbSet<UserModel> user { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrderDetailModel>()
                .HasKey(od => new { od.orderId, od.dishId });

            modelBuilder.Entity<OrderModel>()
                .HasOne(o => o.user)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.userId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DishModel>()
                .HasOne(d => d.category)
                .WithMany(c => c.dish)
                .HasForeignKey(d => d.categoryId)
                .OnDelete(DeleteBehavior.SetNull);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=LAPTOP-571RI1S9;Initial Catalog=FRest;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");
            }
        }
    }
}
