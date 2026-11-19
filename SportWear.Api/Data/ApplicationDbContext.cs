using SportWear.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SportWear.Api.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; } 
        public DbSet<Size> Sizes { get; set; } 
        public DbSet<ProductSize> ProductSizes { get; set; } 


        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductSize>()
                .HasIndex(ps => new { ps.ProductId, ps.SizeId })
                .IsUnique();

            modelBuilder.Entity<Cart>()
                .HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.ProductSize)
                .WithMany()
                .HasForeignKey(ci => ci.ProductSizeId);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.ProductSize)
                .WithMany()
                .HasForeignKey(oi => oi.ProductSizeId);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Cart>()
                .HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<Cart>(c => c.UserId);

            modelBuilder.Entity<Order>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(o => o.UserId);
        }
    }
}
