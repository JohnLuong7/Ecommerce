namespace Ecommerce.Models
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string CategoryName { get; set; }
        public string MainImage { get; set; }
        public bool IsPublished { get; set; }
        public string ShortDescription { get; set; }
    }
}
