using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ecommerce.Models; // Quan trọng để sử dụng các Model hỗ trợ

namespace Ecommerce.Models.ViewModels
{
    // ViewModel này được sử dụng để hiển thị sản phẩm ở Index và Details
    public class ProductViewModel
    {
        public int ProductId { get; set; }

        [Display(Name = "Tên Sản Phẩm")]
        public string Name { get; set; } = string.Empty;

        [DataType(DataType.Currency)]
        [Display(Name = "Giá")]
        public decimal Price { get; set; }

        [Display(Name = "Danh Mục")]
        public string CategoryName { get; set; } = string.Empty;

        [Display(Name = "Ảnh Chính")]
        public string? MainImage { get; set; }

        [Display(Name = "Tồn Kho")]
        public int Quantity { get; set; }

        // ----------------------------------------------------
        // CÁC THUỘC TÍNH JSON ĐƯỢC ĐỒNG BỘ VỚI Product.cs

        // Chứa JSON string của BannerModel
        public string? OfferBannerJson { get; set; }

        // Chứa JSON string của List<ProductVersionModel>
        public string? VariantsJson { get; set; }

        // Chứa JSON string của List<PromotionItemModel>
        public string? PromotionsJson { get; set; }

        // Chứa JSON string của List<ColorOptionModel>
        public string? ColorsJson { get; set; }

        // Chứa JSON string của List<SpecGroupModel>
        public string? SpecsJson { get; set; }
    }
}
