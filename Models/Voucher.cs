using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class Voucher
    {
        [Key]
        public int VoucherID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } // Ví dụ: SUMMER20

        [Required]
        [MaxLength(10)]
        public string DiscountType { get; set; } // 'Percentage' hoặc 'FixedAmount'

        [Column(TypeName = "decimal(10, 2)")]
        public decimal DiscountValue { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MinimumOrderAmount { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        public int? UsageLimit { get; set; } // Dùng int? cho phép null

        public bool IsActive { get; set; }

        // Thêm các quan hệ nếu cần (ví dụ: áp dụng cho các đơn hàng đã dùng voucher này)
        public ICollection<Order> Orders { get; set; }
    }
}
