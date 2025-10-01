using Ecommerce.Data;
using Ecommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Lấy giỏ hàng của user hiện tại hoặc tạo mới
        private async Task<Cart> GetOrCreateCart()
        {
            var userId = _userManager.GetUserId(User);

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    CartItems = new List<CartItem>() // tránh null
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }
            return cart;
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
            var cart = await GetOrCreateCart();
            return View(cart);
        }

        // POST: Cart/Add
        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            if (quantity < 1) quantity = 1;

            var product = await _context.Products.FindAsync(productId);
            if (product == null || !product.IsPublished)
            {
                TempData["ErrorMessage"] = "❌ Sản phẩm không tồn tại hoặc chưa được đăng bán.";
                return RedirectToAction("Index", "Home");
            }

            var cart = await GetOrCreateCart();
            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                _context.CartItems.Update(existingItem);
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = productId,
                    Quantity = quantity
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"✅ Đã thêm {product.Name} vào giỏ hàng!";
            return RedirectToAction("Index", "Home"); // hoặc RedirectToAction("Index", "Cart")
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem == null) return NotFound();

            if (quantity < 1)
            {
                _context.CartItems.Remove(cartItem);
                TempData["SuccessMessage"] = "🗑️ Đã xóa sản phẩm khỏi giỏ.";
            }
            else
            {
                cartItem.Quantity = quantity;
                _context.CartItems.Update(cartItem);
                TempData["SuccessMessage"] = "✏️ Đã cập nhật số lượng sản phẩm.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // POST: Cart/Remove
        [HttpPost]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "🗑️ Đã xóa sản phẩm khỏi giỏ.";
            }
            return RedirectToAction("Index");
        }
    }
}
