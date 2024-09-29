﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace Restaurant.Repository
{
    public class DataContext : IdentityDbContext<UserModel, IdentityRole<long>, long>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<CategoryModel> Category { get; set; }
        public DbSet<DishModel> Dish { get; set; }
        public DbSet<OrderModel> Order { get; set; }
        public DbSet<OrderDetailModel> OrderDetails { get; set; }
        public DbSet<WebSettingModel> Web_setting { get; set; }
        public DbSet<AdsModel> Ads { get; set; }
        public DbSet<BlogModel> Blog { get; set; }
        public DbSet<UserModel> User { get; set; }

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
                optionsBuilder.UseSqlServer("Data Source=mad;Initial Catalog=FRest;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");
            }
        }
    }
}
