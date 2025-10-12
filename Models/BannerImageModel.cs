using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Models
{
    public class BannerImageModel : Controller
    {
        public class BannerModel
        {
            // Đường dẫn hoặc tên file của banner
            public string? ImageUrl { get; set; }

            // Liên kết (URL) khi người dùng click vào banner
            public string? LinkUrl { get; set; }

            // Tên mô tả (alt text) cho banner
            public string? AltText { get; set; }

        }
    }
}
