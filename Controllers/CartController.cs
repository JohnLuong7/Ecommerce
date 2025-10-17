using Ecommerce.Data;
using Ecommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ecommerce.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        private async Task<Cart> GetOrCreateUserCartAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Inventory)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = System.DateTime.UtcNow,
                    CartItems = new List<CartItem>()
                };
            }

            return cart;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var cart = await GetOrCreateUserCartAsync();
            return View(cart);
        }

        [HttpPost("Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int quantity, string action)
        {
            if (quantity <= 0)
            {
                TempData["ErrorMessage"] = "Số lượng không hợp lệ.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            var cart = await GetOrCreateUserCartAsync();
            var product = await _context.Products
                .Include(p => p.Inventory)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null || !product.IsPublished)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại hoặc chưa được đăng bán.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            var existingItem = cart.CartItems.FirstOrDefault(i => i.ProductId == productId);
            int currentCartQuantity = existingItem?.Quantity ?? 0;
            int requestedTotalQuantity = currentCartQuantity + quantity;
            int availableQuantity = product.Inventory?.Quantity ?? 0;

            if (requestedTotalQuantity > availableQuantity)
            {
                TempData["ErrorMessage"] = $"Số lượng yêu cầu ({requestedTotalQuantity}) vượt quá tồn kho ({availableQuantity}).";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                if (cart.CartId == 0)
                {
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                }

                var newItem = new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    CartId = cart.CartId
                };

                cart.CartItems.Add(newItem);
                _context.CartItems.Add(newItem);
            }

            await _context.SaveChangesAsync();

            if (action == "buyNow")
                return RedirectToAction(nameof(Index));

            TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng.";
            return RedirectToAction("Details", "Products", new { id = productId });
        }

        // ✅ HÀM XÓA (AJAX, không reload)
        [HttpPost("RemoveItem")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var item = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.Cart.UserId == userId);

            if (item == null)
                return Json(new { success = false, message = "Không tìm thấy sản phẩm." });

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            decimal totalCart = await _context.CartItems
                .Where(ci => ci.Cart.UserId == userId)
                .SumAsync(ci => (decimal)(ci.Product.Price * ci.Quantity));

            return Json(new { success = true, cartTotal = totalCart.ToString("N0") + " ₫" });
        }

        // ✅ HÀM CẬP NHẬT SỐ LƯỢNG (AJAX, không reload)
        [HttpPost("UpdateQuantity")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var itemToUpdate = await _context.CartItems
                .Include(ci => ci.Product)
                    .ThenInclude(p => p.Inventory)
                .Include(ci => ci.Cart)
                .Where(ci => ci.Cart.UserId == userId)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

            if (itemToUpdate == null)
                return Json(new { success = false, message = "Không tìm thấy sản phẩm." });

            int availableQuantity = itemToUpdate.Product.Inventory?.Quantity ?? 0;

            if (quantity <= 0)
                return Json(new { success = false, message = "Số lượng phải lớn hơn 0." });

            if (quantity > availableQuantity)
                return Json(new { success = false, message = $"Vượt quá tồn kho ({availableQuantity})." });

            itemToUpdate.Quantity = quantity;
            await _context.SaveChangesAsync();

            decimal itemTotal = (decimal)(itemToUpdate.Product.Price * itemToUpdate.Quantity);
            decimal totalCart = await _context.CartItems
                .Where(ci => ci.Cart.UserId == userId)
                .SumAsync(ci => (decimal)(ci.Product.Price * ci.Quantity));

            return Json(new
            {
                success = true,
                itemTotal = itemTotal.ToString("N0") + " ₫",
                cartTotal = totalCart.ToString("N0") + " ₫"
            });
        }
    }
}
