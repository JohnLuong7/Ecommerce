using Ecommerce.Data;
using Ecommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
// using System.Text.Json; // Giữ lại nếu cần, hoặc có thể xóa nếu không dùng

namespace Ecommerce.Controllers
{
    // FIX LỖI 404: Chỉ định rõ ràng tuyến đường (route) cho Controller
    [Route("[controller]")]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // =======================================================
        // GET: Products/Details/5 (BỔ SUNG ACTION NÀY)
        // Route: /Products/Details/5
        // =======================================================
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Tải sản phẩm cùng với Category, Inventory, và ProductImages (cần cho View Details.cshtml)
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Inventory)
                .Include(p => p.ProductImages.OrderBy(pi => pi.IsMain)) // Lấy ảnh, ưu tiên ảnh chính
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // =======================================================
        // GET: Products (Hiển thị danh sách)
        // Route: /Products hoặc /Products/Index
        // =======================================================
        [HttpGet("")]
        [HttpGet("Index")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                                 .Include(p => p.Category)
                                 .ToListAsync();
            return View(products);
        }

        // ... giữ nguyên các Action Delete và DeleteConfirmed khác
        [HttpGet("Delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                                 .Include(p => p.Category)
                                 .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Route("Delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(p => p.ProductId == id);
        }
    }
}