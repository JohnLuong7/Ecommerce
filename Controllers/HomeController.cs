using System.Diagnostics;
using Ecommerce.Data;
using Ecommerce.Models;
using Ecommerce.Models.ViewModels; // Đảm bảo đã include namespace này
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Ecommerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var productsQuery = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                // BẮT BUỘC: Include Inventory để truy cập thông tin tồn kho
                .Include(p => p.Inventory)
                // Tối ưu hóa truy vấn Select để tránh lỗi biên dịch và gọi FirstOrDefault() nhiều lần
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryName = p.Category.Name,
                    // Lấy ảnh chính (nếu có)
                    MainImage = p.ProductImages.FirstOrDefault(img => img.IsMain).FileName,
                    // Lấy số lượng tồn kho một cách an toàn. 
                    // Dùng Select Many hoặc Select(i => i.Quantity) để lấy Quantity.
                    // Nếu p.Inventory là Collection (One-to-Many): 
                    // Quantity = p.Inventory.Select(i => (int?)i.Quantity).FirstOrDefault() ?? 0
                    // Nếu p.Inventory là đối tượng (One-to-One):
                    Quantity = p.Inventory != null
                                ? p.Inventory.Quantity // Truy cập trực tiếp Quantity nếu là One-to-One
                                : 0
                })
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim().ToLower(); // Chuẩn hoá input
                productsQuery = productsQuery.Where(p =>
                    p.Name.ToLower().Contains(searchString) ||
                    p.CategoryName.ToLower().Contains(searchString)
                );
            }

            var products = await productsQuery.ToListAsync();
            return View(products);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult ProductManage()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult UserManage()
        {
            return View();
        }

        [Authorize(Roles = "User")]
        public IActionResult User()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

}
