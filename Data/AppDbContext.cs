using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Models;

namespace Ecommerce.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed Categories
            builder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "Electronics", Slug = "electronics", Description = "Electronic devices and gadgets" },
                new Category { CategoryId = 2, Name = "Books", Slug = "books", Description = "Books and literature" },
                new Category { CategoryId = 3, Name = "Clothing", Slug = "clothing", Description = "Apparel and accessories" }
            );

            // Seed Products - ĐÃ BỔ SUNG CÁC TRƯỜNG MỚI (SalePrice, SpecificationsJson, DetailContentJson)
            builder.Entity<Product>().HasData(
                new Product
                {
                    ProductId = 1,
                    Name = "Smartphone",
                    ShortDescription = "smartphone",
                    Description = "Latest model smartphone",
                    SKU = "Phone-001",
                    Price = 699.99m,
                    CategoryId = 1,
                    // THÊM CÁC TRƯỜNG MỚI:
                    SalePrice = 650.00m, // Giá khuyến mãi
                    SpecificationsJson = null, // Giá trị null
                    DetailContentJson = null   // Giá trị null
                },
                new Product
                {
                    ProductId = 2,
                    Name = "Novel Book",
                    ShortDescription = "novel-book",
                    Description = "Bestselling novel",
                    SKU = "Book-001",
                    Price = 19.99m,
                    CategoryId = 2,
                    // THÊM CÁC TRƯỜNG MỚI:
                    SalePrice = null,
                    SpecificationsJson = null,
                    DetailContentJson = null
                },
                new Product
                {
                    ProductId = 3,
                    Name = "T-Shirt",
                    ShortDescription = "t-shirt",
                    Description = "Cotton t-shirt",
                    SKU = "Tshirt-001",
                    Price = 9.99m,
                    CategoryId = 3,
                    // THÊM CÁC TRƯỜNG MỚI:
                    SalePrice = 9.99m,
                    SpecificationsJson = null,
                    DetailContentJson = null
                }
            );

            // Seed Inventory
            builder.Entity<Inventory>().HasData(
                new Inventory { InventoryId = 1, ProductId = 1, Quantity = 50 },
                new Inventory { InventoryId = 2, ProductId = 2, Quantity = 200 },
                new Inventory { InventoryId = 3, ProductId = 3, Quantity = 150 }
            );
        }
    }
}