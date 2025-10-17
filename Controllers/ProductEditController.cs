using Ecommerce.Data;
using Ecommerce.Models;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ecommerce.Controllers
{
    public class ProductEditController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductEditController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // =======================================================
        // GET: ProductEdit/Edit/5
        // =======================================================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Inventory)
                .Include(p => p.ProductImages.OrderBy(i => i.ProductImageId)) // Order images for consistent display
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            // --- Xử lý JSON An toàn (Giữ nguyên) ---
            List<SpecificationGroupViewModel> specGroups;
            try
            {
                specGroups = JsonSerializer.Deserialize<List<SpecificationGroupViewModel>>(product.SpecificationsJson ?? "[]") ?? new List<SpecificationGroupViewModel>();
            }
            catch (JsonException) { specGroups = new List<SpecificationGroupViewModel>(); }

            List<DetailContentBlockViewModel> detailBlocks;
            try
            {
                detailBlocks = JsonSerializer.Deserialize<List<DetailContentBlockViewModel>>(product.DetailContentJson ?? "[]") ?? new List<DetailContentBlockViewModel>();
            }
            catch (JsonException) { detailBlocks = new List<DetailContentBlockViewModel>(); }

            // --- MAP ProductImages sang ExistingImageViewModel ---
            var existingImages = product.ProductImages
                .Select(i => new ExistingImageViewModel
                {
                    ImageId = i.ProductImageId,
                    ImageUrl = i.FileName, // Lưu tên file để hiển thị và thao tác
                    IsPrimary = string.Equals(i.FileName, product.PrimaryImageUrl, StringComparison.OrdinalIgnoreCase)
                })
                // Sắp xếp ảnh chính lên đầu
                .OrderByDescending(img => img.IsPrimary)
                .ThenBy(img => img.ImageId)
                .ToList();


            // --- MAP PRODUCT MODEL SANG VIEWMDL ---
            var model = new EditProductViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                ShortDescription = product.ShortDescription,
                Description = product.Description,
                Price = product.Price,
                SalePrice = product.SalePrice,
                SKU = product.SKU,
                IsPublished = product.IsPublished,
                CategoryId = product.CategoryId,
                Quantity = product.Inventory?.Quantity ?? 0,

                SpecificationGroups = specGroups,
                DetailContentBlocks = detailBlocks,

                ExistingImages = existingImages // Dòng này thay thế logic MainImageId cũ
            };

            var categories = await _context.Categories.ToListAsync() ?? new List<Category>();
            ViewData["Categories"] = new SelectList(categories, "CategoryId", "Name", model.CategoryId);

            return View("~/Views/Products/Edit.cshtml", model);
        }

        // =======================================================
        // POST: ProductEdit/Edit/5 (Lưu thay đổi)
        // =======================================================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditProductViewModel model)
        {
            if (id != model.ProductId) return NotFound();

            var existingProduct = await _context.Products
                .Include(p => p.Inventory)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (existingProduct == null) return NotFound();

            // Loại bỏ các validation cho file và JSON phức tạp nếu chúng không được submit
            ModelState.Remove(nameof(model.Images));
            ModelState.Remove(nameof(model.SpecificationGroups));
            ModelState.Remove(nameof(model.DetailContentBlocks));

            model.SpecificationGroups ??= new List<SpecificationGroupViewModel>();
            model.DetailContentBlocks ??= new List<DetailContentBlockViewModel>();

            if (ModelState.IsValid)
            {
                try
                {
                    // --- BƯỚC 1: SERIALIZE DỮ LIỆU PHỨC TẠP SANG JSON TRƯỚC KHI LƯU ---
                    var specsJson = JsonSerializer.Serialize(model.SpecificationGroups);
                    var detailJson = JsonSerializer.Serialize(model.DetailContentBlocks);

                    // --- BƯỚC 2: CẬP NHẬT PRODUCT MODEL ---
                    existingProduct.Name = model.Name;
                    existingProduct.ShortDescription = model.ShortDescription;
                    existingProduct.Description = model.Description;
                    existingProduct.Price = model.Price;
                    existingProduct.SalePrice = model.SalePrice;
                    existingProduct.SKU = model.SKU;
                    existingProduct.IsPublished = model.IsPublished;
                    existingProduct.CategoryId = model.CategoryId;
                    existingProduct.SpecificationsJson = specsJson;
                    existingProduct.DetailContentJson = detailJson;

                    // --- BƯỚC 3: CẬP NHẬT INVENTORY ---
                    if (existingProduct.Inventory != null)
                    {
                        existingProduct.Inventory.Quantity = model.Quantity;
                        _context.Update(existingProduct.Inventory);
                    }
                    else
                    {
                        _context.Inventory.Add(new Inventory { ProductId = existingProduct.ProductId, Quantity = model.Quantity });
                    }

                    // --- BƯỚC 4: XỬ LÝ ẢNH MỚI ---
                    ProductImage? firstNewImage = null;

                    if (model.Images != null && model.Images.Count > 0)
                    {
                        var imageFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                        if (!Directory.Exists(imageFolder)) Directory.CreateDirectory(imageFolder);

                        foreach (var image in model.Images)
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
                                    ProductId = existingProduct.ProductId,
                                    FileName = fileName,
                                    IsMain = false
                                };
                                existingProduct.ProductImages.Add(productImage);

                                if (firstNewImage == null) firstNewImage = productImage;
                            }
                        }
                    }

                    // --- BƯỚC 5: XỬ LÝ ẢNH CHÍNH (Chỉ khi chưa có ảnh chính VÀ có ảnh mới/ảnh cũ còn sót) ---

                    // Nếu sản phẩm chưa có ảnh chính (hoặc ảnh chính cũ đã bị xóa qua AJAX)
                    // Dòng 221 (dòng lỗi CS1061 cũ) đã được loại bỏ và thay thế bằng logic dưới đây
                    if (string.IsNullOrEmpty(existingProduct.PrimaryImageUrl))
                    {
                        // 5.1. Ưu tiên chọn ảnh mới được upload làm ảnh chính nếu không có ảnh cũ nào
                        if (firstNewImage != null)
                        {
                            existingProduct.PrimaryImageUrl = firstNewImage.FileName;
                            firstNewImage.IsMain = true;
                        }
                        else
                        {
                            // 5.2. Nếu không có ảnh mới, chọn ảnh đầu tiên còn lại (nếu có)
                            // Sử dụng ProductImages.Where(i => i.ProductImageId > 0) để chỉ lấy ảnh đã có trong DB
                            var allImages = existingProduct.ProductImages.Where(i => i.ProductImageId > 0).ToList();
                            var firstRemainingImage = allImages.OrderBy(i => i.ProductImageId).FirstOrDefault();

                            if (firstRemainingImage != null)
                            {
                                existingProduct.PrimaryImageUrl = firstRemainingImage.FileName;
                                firstRemainingImage.IsMain = true;
                            }
                            else
                            {
                                // Không còn ảnh nào
                                existingProduct.PrimaryImageUrl = null;
                            }
                        }
                    }
                    // Nếu đã có PrimaryImageUrl (do đã đặt qua AJAX), thì giữ nguyên.

                    _context.Update(existingProduct);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(model.ProductId)) return NotFound();
                    else throw;
                }

                return RedirectToAction("Index", "Products");
            }

            // --- Load lại dữ liệu cho View nếu ModelState không hợp lệ ---
            var categoriesOnFailure = await _context.Categories.ToListAsync() ?? new List<Category>();
            ViewData["Categories"] = new SelectList(categoriesOnFailure, "CategoryId", "Name", model.CategoryId);

            // Tải lại ExistingImages cho model nếu ModelState không hợp lệ
            var images = await _context.ProductImages.Where(i => i.ProductId == id).ToListAsync();
            model.ExistingImages = images
                .Select(i => new ExistingImageViewModel
                {
                    ImageId = i.ProductImageId,
                    ImageUrl = i.FileName,
                    IsPrimary = string.Equals(i.FileName, existingProduct.PrimaryImageUrl, StringComparison.OrdinalIgnoreCase)
                })
                .OrderByDescending(img => img.IsPrimary)
                .ThenBy(img => img.ImageId)
                .ToList();

            return View("~/Views/Products/Edit.cshtml", model);
        }

        // =======================================================
        // POST: ProductEdit/DeleteImage (Sử dụng AJAX)
        // =======================================================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteImage(int imageId, int productId)
        {
            var image = await _context.ProductImages.FindAsync(imageId);
            if (image == null)
            {
                return Json(new { success = false, message = "Không tìm thấy ảnh.", isPrimaryRemoved = false });
            }

            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null) return Json(new { success = false, message = "Không tìm thấy sản phẩm.", isPrimaryRemoved = false });

            bool wasPrimary = string.Equals(product.PrimaryImageUrl, image.FileName, StringComparison.OrdinalIgnoreCase);

            // 1. Xóa file khỏi hệ thống (ổ đĩa)
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", image.FileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // 2. Xóa khỏi cơ sở dữ liệu
            _context.ProductImages.Remove(image);

            // Cập nhật lại danh sách trong bộ nhớ (entity framework tracking)
            product.ProductImages.Remove(image);

            string? newPrimaryUrl = null;
            int? newPrimaryId = null;

            // 3. CẬP NHẬT ẢNH CHÍNH NẾU ẢNH BỊ XÓA LÀ ẢNH CHÍNH
            if (wasPrimary)
            {
                // Tìm ảnh đầu tiên còn lại
                var remainingImages = product.ProductImages
                    .OrderBy(i => i.ProductImageId)
                    .ToList();

                var newPrimaryImage = remainingImages.FirstOrDefault();

                product.PrimaryImageUrl = newPrimaryImage?.FileName;
                newPrimaryUrl = product.PrimaryImageUrl;
                newPrimaryId = newPrimaryImage?.ProductImageId;

                _context.Update(product);

                // Cập nhật cờ IsMain
                if (newPrimaryImage != null)
                {
                    newPrimaryImage.IsMain = true;
                    _context.Update(newPrimaryImage).Property(p => p.IsMain).IsModified = true;
                }
            }

            await _context.SaveChangesAsync();

            // Trả về kết quả JSON để cập nhật UI
            return Json(new { success = true, isPrimaryRemoved = wasPrimary, newPrimaryUrl = newPrimaryUrl, newPrimaryId = newPrimaryId });
        }

        // =======================================================
        // POST: ProductEdit/SetMainImage (Sử dụng AJAX)
        // =======================================================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetMainImage(int imageId, int productId)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null) return Json(new { success = false, message = "Không tìm thấy sản phẩm." });

            var targetImage = product.ProductImages.FirstOrDefault(i => i.ProductImageId == imageId);

            if (targetImage == null)
            {
                return Json(new { success = false, message = "Không tìm thấy ảnh mục tiêu." });
            }

            // 1. Reset trạng thái IsMain cho ảnh cũ
            foreach (var img in product.ProductImages)
            {
                if (string.Equals(img.FileName, product.PrimaryImageUrl, StringComparison.OrdinalIgnoreCase) && img.ProductImageId != imageId)
                {
                    img.IsMain = false;
                    _context.Update(img).Property(p => p.IsMain).IsModified = true;
                    break;
                }
            }

            // 2. Đặt ảnh mục tiêu làm ảnh chính
            targetImage.IsMain = true;
            _context.Update(targetImage).Property(p => p.IsMain).IsModified = true;

            // 3. Cập nhật PrimaryImageUrl của Product
            product.PrimaryImageUrl = targetImage.FileName;
            _context.Update(product);

            await _context.SaveChangesAsync();

            // Trả về Json cho AJAX request
            return Json(new { success = true, newPrimaryUrl = targetImage.FileName, newPrimaryId = imageId });
        }


        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
