using Ecommerce.Data;
using Ecommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;
        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Products
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            // Include Category để load tên Category lên view
            var products = await _context.Products
                                .Include(p => p.Category)
                                .ToListAsync();
            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                        .Include(p => p.Category)
                        .Include(p => p.Inventory)
                        .Include(p => p.ProductImages)
                        .FirstOrDefaultAsync(p => p.ProductId == id);


            if (product == null) return NotFound();

            return View(product);
        }

        // GET: Products/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            // Truyền danh sách Category để chọn trong dropdown
            ViewData["Categories"] = _context.Categories.ToList();
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, List<IFormFile> images, int quantity)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Categories"] = _context.Categories.ToList();
                return View(product);
            }

            _context.Add(product);
            await _context.SaveChangesAsync();

            // Tạo Inventory ngay sau khi product đã có Id
            var inventory = new Inventory
            {
                ProductId = product.ProductId,
                Quantity = quantity
            };
            _context.Inventory.Add(inventory);
            await _context.SaveChangesAsync();

            // Xử lý ảnh như trước
            if (images != null && images.Count > 0)
            {
                var imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(imageFolder))
                {
                    Directory.CreateDirectory(imageFolder);
                }

                bool firstImage = true;
                foreach (var image in images)
                {
                    if (image.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                        var filePath = Path.Combine(imageFolder, fileName);

                        using var stream = new FileStream(filePath, FileMode.Create);
                        await image.CopyToAsync(stream);

                        var productImage = new ProductImage
                        {
                            ProductId = product.ProductId,
                            FileName = fileName,
                            IsMain = firstImage
                        };
                        firstImage = false;

                        _context.ProductImages.Add(productImage);
                    }
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }





        // GET: Products/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewData["Categories"] = _context.Categories.ToList();
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, List<IFormFile> newImages)
        {
            if (id != product.ProductId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    if (newImages != null && newImages.Count > 0)
                    {
                        foreach (var image in newImages)
                        {
                            if (image.Length > 0)
                            {
                                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await image.CopyToAsync(stream);
                                }

                                var productImage = new ProductImage
                                {
                                    ProductId = product.ProductId,
                                    FileName = fileName,
                                    IsMain = false
                                };

                                _context.ProductImages.Add(productImage);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Categories"] = _context.Categories.ToList();
            return View(product);
        }


        // GET: Products/Delete/5
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

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
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
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var image = await _context.ProductImages.FindAsync(imageId);
            if (image != null)
            {
                // Xóa file ảnh trên disk
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", image.FileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.ProductImages.Remove(image);
                await _context.SaveChangesAsync();
            }

            // Redirect về Edit sản phẩm tương ứng
            return RedirectToAction("Edit", new { id = image.ProductId });
        }


        private bool ProductExists(int id)
        {
            return _context.Products.Any(p => p.ProductId == id);
        }

    }
}
