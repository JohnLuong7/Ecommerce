namespace Ecommerce.Models
{
    public class ProductVersionModel
    {
        public int Id { get; set; }
        public string? Name { get; set; } // Ví dụ: 8GB/256GB
        public decimal Price { get; set; }
        // Có thể thêm OriginalPrice, StockQuantity, v.v.
    }
}