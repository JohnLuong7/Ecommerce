using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class ActivityLog
    {
        [Key]
        public int LogID { get; set; }

        // Khóa ngoại liên kết với AspNetUsers
        public string UserID { get; set; }
        // [ForeignKey("UserID")]
        // public YourApplicationUser User { get; set; } // Navigation Property (Tùy chọn)

        [Required]
        [MaxLength(50)]
        public string ActivityType { get; set; } // Ví dụ: 'ViewProduct', 'AddToCart'

        // Khóa ngoại liên kết với Product (Nullable vì không phải mọi hoạt động đều liên quan đến sản phẩm)
        public int? ProductID { get; set; }
        [ForeignKey("ProductID")]
        public Product Product { get; set; } // Navigation Property

        [MaxLength(4000)]
        public string Details { get; set; } // Lưu trữ chi tiết (ví dụ: 'tìm kiếm: áo phông')

        public DateTime Timestamp { get; set; }
    }
}
