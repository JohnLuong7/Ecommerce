using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class ProductReview
    {
        [Key]
        public int ReviewID { get; set; }

        // Khóa ngoại liên kết với Product
        public int ProductID { get; set; }
        [ForeignKey("ProductID")]
        public Product Product { get; set; } // Navigation Property

        // Khóa ngoại liên kết với AspNetUsers (Khách hàng)
        // Giả sử kiểu dữ liệu của UserID trong AspNetUsers là string
        public string UserID { get; set; }
        // [ForeignKey("UserID")]
        // public YourApplicationUser User { get; set; } // Navigation Property (Tùy chọn)

        [Required]
        [Range(1, 5)] // Đánh giá từ 1 đến 5 sao
        public int Rating { get; set; }

        [MaxLength(4000)] // Chiều dài tối đa 4000 ký tự
        public string Comment { get; set; }

        public DateTime ReviewDate { get; set; }
    }
}
