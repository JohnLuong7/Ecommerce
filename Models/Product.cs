using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Ecommerce.Models
{
    public class Product
    {
        // Thuộc tính chính
        [Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc.")]
        [StringLength(255)]
        [Display(Name = "Tên Sản Phẩm")]
        public string Name { get; set; }

        [Display(Name = "Mã Sản Phẩm (SKU)")]
        [StringLength(50)]
        public string? Sku { get; set; }

        // MÔ TẢ (Đã thêm ShortDescription để fix lỗi CS1061)
        [Display(Name = "Mô tả ngắn")]
        public string? ShortDescription { get; set; } // KHẮC PHỤC LỖI CS1061

        [Display(Name = "Mô tả chi tiết")]
        public string? Description { get; set; }

        // GIÁ
        [Required(ErrorMessage = "Giá bán là bắt buộc.")]
        [Column(TypeName = "decimal(18, 0)")]
        [Display(Name = "Giá Bán")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18, 0)")]
        [Display(Name = "Giá Gốc (nếu có khuyến mãi)")]
        public decimal? OriginalPrice { get; set; }

        // TRẠNG THÁI
        [Display(Name = "Hiển thị công khai")]
        public bool IsPublished { get; set; } = true;

        [Display(Name = "Ngày Tạo")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;


        // KHU VỰC JSON (Variants, Colors, Promotions, Specs, Commitments)
        // Đây là các trường lưu trữ chuỗi JSON, được Deserialize ở file Details.cshtml

        [Column(TypeName = "nvarchar(MAX)")]
        public string? VariantsJson { get; set; } // Dùng cho versions

        [Column(TypeName = "nvarchar(MAX)")]
        public string? ColorsJson { get; set; }

        [Column(TypeName = "nvarchar(MAX)")]
        public string? PromotionsJson { get; set; }

        [Column(TypeName = "nvarchar(MAX)")]
        public string? OfferBannerJson { get; set; }

        [Column(TypeName = "nvarchar(MAX)")]
        public string? SpecsJson { get; set; }

        [Column(TypeName = "nvarchar(MAX)")]
        public string? CommitmentsJson { get; set; }


        // Mối quan hệ Navigation

        // 1. Category
        [Display(Name = "Danh mục")]
        public int? CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; } // Giả định bạn có Model Category

        // 2. Product Images (Danh sách ảnh)
        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>(); // Giả định bạn có Model ProductImage

        // 3. Inventory (Tồn kho)
        // Đây là thuộc tính dẫn đến lỗi CS0229, cần dùng biến cục bộ để tránh lỗi
        public Inventory? Inventory { get; set; } // Giả định bạn có Model Inventory
    }
}