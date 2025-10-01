using Ecommerce.Data;
using Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Ecommerce.Controllers
{
    public class InventoryController : Controller
    {
        private readonly AppDbContext _context;

        public InventoryController(AppDbContext context)
        {
            _context = context;
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
        public IActionResult Create()
        {
            ViewData["Products"] = new SelectList(_context.Products, "ProductId", "Name");
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
            ViewData["Products"] = new SelectList(_context.Products, "ProductId", "Name", inventory.ProductId);
            return View(inventory);
        }

        // GET: Inventory/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var inventory = await _context.Inventory.FindAsync(id);
            if (inventory == null) return NotFound();

            ViewData["Products"] = new SelectList(_context.Products, "ProductId", "Name", inventory.ProductId);
            return View(inventory);
        }

        // POST: Inventory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Inventory inventory)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                               .SelectMany(v => v.Errors)
                               .Select(e => e.ErrorMessage)
                               .ToList();

                ViewBag.Errors = errors;

                ViewData["Categories"] = _context.Categories.ToList(); // hoặc ViewBag, tùy bạn

                return View(inventory); // hoặc model tương ứng
            }


            if (ModelState.IsValid)
            {
                try
                {
                    var existingInventory = await _context.Inventory.FindAsync(id);
                    if (existingInventory == null) return NotFound();

                    // Gán từng trường cần cập nhật
                    existingInventory.ProductId = inventory.ProductId;
                    existingInventory.Quantity = inventory.Quantity;

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
            ViewData["Products"] = new SelectList(_context.Products, "ProductId", "Name", inventory.ProductId);
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
