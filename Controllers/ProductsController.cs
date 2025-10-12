using Ecommerce.Data;
using Ecommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;
        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Products (Index)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
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

            // Gán CreatedDate nếu chưa được gán
            if (product.CreatedDate == default(DateTime))
            {
                product.CreatedDate = DateTime.Now;
            }

            _context.Add(product);
            await _context.SaveChangesAsync();

            // Tạo Inventory
            var inventory = new Inventory
            {
                ProductId = product.ProductId,
                Quantity = quantity
            };
            _context.Inventory.Add(inventory);
            await _context.SaveChangesAsync();

            // Xử lý ảnh
            if (images != null && images.Count > 0)
            {
                var imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(imageFolder)) Directory.CreateDirectory(imageFolder);

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

            var product = await _context.Products
                                .Include(p => p.ProductImages)
                                .Include(p => p.Inventory)
                                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            ViewData["Categories"] = _context.Categories.ToList();
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, List<IFormFile> newImages, int quantity)
        {
            if (id != product.ProductId)
                return NotFound();

            // Phải đảm bảo CreatedDate được giữ nguyên
            var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == id);
            if (existingProduct != null)
            {
                product.CreatedDate = existingProduct.CreatedDate;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Cập nhật thông tin Product
                    _context.Update(product);

                    // 2. CẬP NHẬT TỒN KHO
                    var inventory = await _context.Inventory.FirstOrDefaultAsync(i => i.ProductId == id);
                    if (inventory != null)
                    {
                        inventory.Quantity = quantity;
                        _context.Inventory.Update(inventory);
                    }
                    else
                    {
                        _context.Inventory.Add(new Inventory { ProductId = id, Quantity = quantity });
                    }

                    await _context.SaveChangesAsync();

                    // 3. XỬ LÝ ẢNH MỚI
                    if (newImages != null && newImages.Count > 0)
                    {
                        var imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                        if (!Directory.Exists(imageFolder)) Directory.CreateDirectory(imageFolder);

                        foreach (var image in newImages)
                        {
                            if (image.Length > 0)
                            {
                                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                                var filePath = Path.Combine(imageFolder, fileName);

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
                    // Lỗi CS0103 xảy ra ở đây
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
            if (image == null) return NotFound();

            var productId = image.ProductId;

            // Xóa file ảnh trên disk
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", image.FileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            return RedirectToAction("Edit", new { id = productId });
        }

        // PHƯƠNG THỨC ĐÃ THÊM: KHẮC PHỤC LỖI CS0103
        private bool ProductExists(int id)
        {
            return _context.Products.Any(p => p.ProductId == id);
        }

    }
}