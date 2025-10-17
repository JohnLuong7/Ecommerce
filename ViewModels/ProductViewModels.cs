using Ecommerce.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.ViewModels
{
    // ===============================================
    // VIEWMDL CON DÙNG TRONG EDIT
    // Dùng để hiển thị và quản lý các ảnh đã có sẵn
    // ===============================================
    public class ExistingImageViewModel
    {
        public int ImageId { get; set; } // Dùng để xóa ảnh phụ
        public string ImageUrl { get; set; } = null!;
        public bool IsPrimary { get; set; } // <<<< THUỘC TÍNH MỚI: Đánh dấu ảnh chính
    }

    // ===============================================
    // CÁC VIEWMDL PHỤ CHO DỮ LIỆU JSON
    // ===============================================
    public class SpecificationItemViewModel
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class SpecificationGroupViewModel
    {
        public string GroupTitle { get; set; } = string.Empty;
        public List<SpecificationItemViewModel> Specifications { get; set; } = new List<SpecificationItemViewModel>();
    }

    public class DetailContentBlockViewModel
    {
        public string BlockType { get; set; } = "text";
        public string Content { get; set; } = string.Empty;
    }

    // ===============================================
    // VIEWMDL CƠ SỞ (BASE)
    // ===============================================
    public abstract class BaseProductViewModel
    {
        [Required(ErrorMessage = "Tên sản phẩm không được để trống"), MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ShortDescription { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Danh mục không được để trống")]
        public int CategoryId { get; set; }

        [Display(Name = "Xuất bản")]
        public bool IsPublished { get; set; }

        [MaxLength(50)]
        public string? SKU { get; set; }

        [Required(ErrorMessage = "Giá niêm yết không được để trống")]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }

        public decimal? SalePrice { get; set; }

        [Required(ErrorMessage = "Số lượng tồn kho không được để trống")]
        [Range(0, 99999, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
        public int Quantity { get; set; }

        public string? Specifications { get; set; }
        public string? DetailContentJson { get; set; }
    }

    // ===============================================
    // VIEWMDL CREATE
    // ===============================================
    public class CreateProductViewModel : BaseProductViewModel
    {
        [Display(Name = "Ảnh sản phẩm")]
        [Required(ErrorMessage = "Vui lòng tải lên ít nhất một hình ảnh")]
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    }

    // ===============================================
    // VIEWMDL EDIT
    // ===============================================
    public class EditProductViewModel : BaseProductViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Display(Name = "Ảnh sản phẩm mới")]
        public List<IFormFile>? Images { get; set; }

        public List<SpecificationGroupViewModel> SpecificationGroups { get; set; } = new List<SpecificationGroupViewModel>();
        public List<DetailContentBlockViewModel> DetailContentBlocks { get; set; } = new List<DetailContentBlockViewModel>();

        // <<< THUỘC TÍNH MainImageId (từ phiên bản trước) ĐÃ ĐƯỢC BỎ QUA >>>

        // <<< THUỘC TÍNH MỚI: Chứa cả ảnh chính và ảnh phụ, dùng để hiển thị và quản lý trong form Edit >>>
        // Đây là danh sách các ảnh đã có sẵn, được map từ ProductImage entity
        [Display(Name = "Ảnh hiện có")]
        public List<ExistingImageViewModel> ExistingImages { get; set; } = new List<ExistingImageViewModel>();
    }

    // ===============================================
    // VIEWMDL DÙNG CHO TRANG INDEX/DANH SÁCH
    // ===============================================
    public class ProductViewModel
    {
        public int ProductId { get; set; }

        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; } = null!;

        [Display(Name = "Giá")]
        public decimal Price { get; set; }

        [Display(Name = "Danh mục")]
        public string CategoryName { get; set; } = null!;

        // (*) THUỘC TÍNH ẢNH CẦN THIẾT CHO TRANG INDEX
        public string? MainImage { get; set; }
    }
}
