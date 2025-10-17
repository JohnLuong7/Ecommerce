using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Models
{
    public class PromotionBannerModel
    {
        // Đường dẫn hoặc tên file của ảnh banner
        public string? ImageUrl { get; set; }

        // Liên kết (URL) khi người dùng click vào banner (CTA Link)
        public string? LinkUrl { get; set; }

        // Tiêu đề hoặc Alt Text cho banner
        public string? Title { get; set; }

        // Loại banner (ví dụ: 'Giảm giá', 'Trả góp 0%', 'Quà tặng') - Hữu ích cho phân loại.
        public string? Type { get; set; }

        // Thứ tự ưu tiên hiển thị (nếu có nhiều banner)
        public int Order { get; set; }
    }

}
