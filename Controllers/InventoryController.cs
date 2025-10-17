using Ecommerce.Data;
using Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq; // Cần cho SelectMany và ToList
using System.Collections.Generic; // Cần cho List<Product>
using System; // Cần cho Guid/Console.WriteLine nếu cần debug

namespace Ecommerce.Controllers
{
    public class InventoryController : Controller
    {
        private readonly AppDbContext _context;

        public InventoryController(AppDbContext context)
        {
            _context = context;
        }

        // Phương thức trợ giúp để đảm bảo danh sách Products không bao giờ null
        private async Task<List<Product>> GetProductsSafeAsync()
        {
            // Sử dụng ?? new List<Product>() để đảm bảo trả về List<Product> rỗng nếu Products là null
            return await _context.Products?.ToListAsync() ?? new List<Product>();
        }


        // GET: Inventory
        public async Task<IActionResult> Index()
        {
            var inventories = await _context.Inventory
                                       .Include(i => i.Product)
                                       .ToListAsync();
            return View(inventories);
        }

        // GET: Inventory/Create
        public async Task<IActionResult> Create()
        {
            // SỬA: Dùng phương thức an toàn để tránh ArgumentNullException
            ViewData["Products"] = new SelectList(await GetProductsSafeAsync(), "ProductId", "Name");
            return View();
        }

        // POST: Inventory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(inventory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // SỬA: Dùng phương thức an toàn khi quay lại View do lỗi
            ViewData["Products"] = new SelectList(await GetProductsSafeAsync(), "ProductId", "Name", inventory.ProductId);
            return View(inventory);
        }

        // GET: Inventory/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var inventory = await _context.Inventory.FindAsync(id);
            if (inventory == null) return NotFound();

            // SỬA: Dùng phương thức an toàn để tránh ArgumentNullException
            ViewData["Products"] = new SelectList(await GetProductsSafeAsync(), "ProductId", "Name", inventory.ProductId);
            return View(inventory);
        }

        // POST: Inventory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Inventory inventory)
        {
            if (id != inventory.InventoryId) return NotFound();

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                               .SelectMany(v => v.Errors)
                               .Select(e => e.ErrorMessage)
                               .ToList();

                ViewBag.Errors = errors;

                // SỬA: Dùng phương thức an toàn khi quay lại View do lỗi
                ViewData["Products"] = new SelectList(await GetProductsSafeAsync(), "ProductId", "Name", inventory.ProductId);

                return View(inventory);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // === LOGIC CẬP NHẬT TỐT HƠN ===
                    // Gắn đối tượng Inventory được bind từ form vào context
                    _context.Update(inventory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Inventory.Any(e => e.InventoryId == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // SỬA: Dùng phương thức an toàn
            ViewData["Products"] = new SelectList(await GetProductsSafeAsync(), "ProductId", "Name", inventory.ProductId);
            return View(inventory);
        }


        // GET: Inventory/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var inventory = await _context.Inventory
                                         .Include(i => i.Product)
                                         .FirstOrDefaultAsync(m => m.InventoryId == id);

            if (inventory == null) return NotFound();

            return View(inventory);
        }

        // POST: Inventory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inventory = await _context.Inventory.FindAsync(id);
            if (inventory != null)
            {
                _context.Inventory.Remove(inventory);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
