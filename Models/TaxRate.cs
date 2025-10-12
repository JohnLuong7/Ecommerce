using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class TaxRate
    {
        [Key]
        public int TaxRateID { get; set; }

        [Required]
        [MaxLength(100)]
        public string RegionName { get; set; } // Ví dụ: 'Miền Bắc', 'TP.HCM'

        [Column(TypeName = "decimal(5, 2)")]
        public decimal VATRate { get; set; } // Mức thuế VAT (ví dụ: 8.00 cho 8%)

        [Column(TypeName = "decimal(10, 2)")]
        public decimal ShippingFee { get; set; } // Phí vận chuyển

        public bool IsDefault { get; set; } // Mức áp dụng mặc định

        // Có thể thêm quan hệ với Order nếu mỗi Order cần lưu TaxRateID đã áp dụng
    }
}
