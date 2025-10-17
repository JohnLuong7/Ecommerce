using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Ecommerce.ViewModels
{
    // ViewModel dùng để truyền dữ liệu giữa View Edit.cshtml và UserManagementController
    public class EditUserViewModel
    {
        // Cần ID để xác định người dùng cần cập nhật (dùng trong POST)
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ và tên không được để trống")]
        [Display(Name = "Họ và tên")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        // Có thể thêm các thuộc tính khác như Phone, Address nếu cần chỉnh sửa
    }
}