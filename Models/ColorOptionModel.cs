// Trong tệp ColorOptionModel.cs
namespace Ecommerce.Models
{
    public class ColorOptionModel
    {
        // Tên hiển thị của màu sắc (ví dụ: "Đen", "Xanh Dương")
        public string? Name { get; set; }

        // Mã Hex của màu sắc để hiển thị (ví dụ: "#000000" cho Đen)
        public string? HexCode { get; set; }
    }
}