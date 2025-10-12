namespace Ecommerce.Models
{
    public class SpecGroupModel
    {
        public string? Name { get; set; } // Ví dụ: Màn hình, Camera
        public List<SpecItemModel>? Items { get; set; } = new List<SpecItemModel>();
    }

    public class SpecItemModel
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
    }
}