using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Models;
using System; // Cần cho DateTime

namespace Ecommerce.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // --- Các DbSet ---
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        // --- Bổ sung 4 DbSet mới ---
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<TaxRate> TaxRates { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // --- CẤU HÌNH QUAN HỆ VÀ KHÓA NGOẠI ---

            // 1. Cấu hình Khóa ngoại Product - Category (Để củng cố nullable)
            // Lệnh này đảm bảo rằng CategoryId trong Products là nullable (vì int?) 
            // và ngăn chặn lỗi cascading delete nếu Category bị xóa.
            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .IsRequired(false) // Xác nhận CategoryId là nullable
                .OnDelete(DeleteBehavior.SetNull); // Nếu Category bị xóa, CategoryId của Product sẽ thành NULL

            // 2. Cấu hình Khóa ngoại CartItem - Product (Ngăn chặn CASCADE DELETE)
            builder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // THAY ĐỔI: Ngăn chặn xóa Product nếu có CartItem tồn tại.

            // ... (Cấu hình TaxRate và Voucher)

            // Seed Categories (Đã đúng)
            builder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "Electronics", Slug = "electronics", Description = "Electronic devices and gadgets" },
                new Category { CategoryId = 2, Name = "Books", Slug = "books", Description = "Books and literature" },
                new Category { CategoryId = 3, Name = "Clothing", Slug = "clothing", Description = "Apparel and accessories" }
            );

            // Seed Products
            builder.Entity<Product>().HasData(
                new Product
                {
                    ProductId = 1,
                    Name = "Smartphone",
                    ShortDescription = "Latest model smartphone",
                    Description = "Latest model smartphone",
                    Sku = "Phone-001",
                    Price = 699.99m,
                    CategoryId = 1,
                    IsPublished = true,
                    CreatedDate = new DateTime(2025, 10, 1)
                },
                new Product
                {
                    ProductId = 2,
                    Name = "Novel Book",
                    ShortDescription = "Bestselling novel",
                    Description = "Bestselling novel",
                    Sku = "Book-001",
                    Price = 19.99m,
                    CategoryId = 2,
                    IsPublished = true,
                    CreatedDate = new DateTime(2025, 10, 1)
                },
                new Product
                {
                    ProductId = 3,
                    Name = "T-Shirt",
                    ShortDescription = "t-shirt",
                    Description = "Cotton t-shirt",
                    Sku = "Tshirt-001",
                    Price = 9.99m,
                    CategoryId = 3,
                    IsPublished = true,
                    CreatedDate = new DateTime(2025, 10, 1)
                }
            );

            // Seed Inventory 
            builder.Entity<Inventory>().HasData(
                new Inventory { InventoryId = 1, ProductId = 1, Quantity = 50 },
                new Inventory { InventoryId = 2, ProductId = 2, Quantity = 200 },
                new Inventory { InventoryId = 3, ProductId = 3, Quantity = 150 }
            );

            // ... (Các Seed Data khác nếu có)
        }
    }
}