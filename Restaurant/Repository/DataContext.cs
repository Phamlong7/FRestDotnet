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
        public DbSet<CommentModel> comment { get; set; }


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

            modelBuilder.Entity<BlogModel>()
                .HasMany(b => b.Comments)
                .WithOne(c => c.Blog)
                .HasForeignKey(c => c.BlogId);

            modelBuilder.Entity<CommentModel>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(GetConnectionString());
        }
        private string GetConnectionString()
        {
            IConfiguration config = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", true, true)
                        .Build();
            var strConn = config["ConnectionStrings:ConnectedDb"];

            return strConn;
        }
    }
}
