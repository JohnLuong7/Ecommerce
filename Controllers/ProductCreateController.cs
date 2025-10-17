using Ecommerce.Data;
using Ecommerce.Models;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Text.Json; // Cần thiết để Serialize JSON

namespace Ecommerce.Controllers
{
    // Controller mới chỉ dành cho hành động tạo sản phẩm
    public class ProductCreateController : Controller
    {
        private readonly AppDbContext _context;
        // Tùy chọn: Thêm IWebHostEnvironment nếu bạn muốn quản lý đường dẫn wwwroot tốt hơn
        // private readonly IWebHostEnvironment _webHostEnvironment; 

        public ProductCreateController(AppDbContext context) // Có thể thêm IWebHostEnvironment
        {
            _context = context;
        }

        // GET: ProductCreate/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            // Truyền danh sách Category để chọn trong dropdown
            ViewData["Categories"] = _context.Categories.ToList();

            // SỬA ĐỔI ĐƯỜNG TRUYỀN: Chỉ định rõ đường dẫn View để sử dụng file Create.cshtml trong thư mục Views/Products
            return View("~/Views/Products/Create.cshtml", new CreateProductViewModel());
        }

        // POST: ProductCreate/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        // NHẬN CreateProductViewModel
        public async Task<IActionResult> Create(CreateProductViewModel model)
        {
            // Tạm thời loại bỏ validation vì form nhập liệu JSON/dynamic cần xử lý phức tạp
            // Nếu bạn muốn giữ validation, hãy đảm bảo các trường cơ bản trong model đã hợp lệ.
            if (!ModelState.IsValid)
            {
                ViewData["Categories"] = _context.Categories.ToList();

                // SỬA ĐỔI ĐƯỜNG TRUYỀN KHI TRẢ VỀ VIEW LẠI
                return View("~/Views/Products/Create.cshtml", model);
            }

            // --- BƯỚC 1: SERIALIZE DỮ LIỆU PHỨC TẠP SANG JSON ---
            // Sử dụng JsonSerializer để chuyển List<ViewModel> thành chuỗi JSON
            var specsJson = JsonSerializer.Serialize(model.Specifications);
            var detailJson = JsonSerializer.Serialize(model.DetailContentJson);

            // LƯU Ý: Nếu DetailContentBlocks có IFormFile, bạn cần xử lý upload file ảnh chi tiết ở đây
            // và cập nhật ImageUrl trong DetailContentBlockViewModel trước khi serialize.

            // --- BƯỚC 2: MAP VIEWMDL SANG PRODUCT MODEL ---
            var product = new Product
            {
                Name = model.Name,
                ShortDescription = model.ShortDescription,
                Description = model.Description, // Có thể bỏ nếu dùng DetailContentJson hoàn toàn
                Price = model.Price,
                SalePrice = model.SalePrice, // Giá khuyến mãi
                SKU = model.SKU,
                IsPublished = model.IsPublished,
                CategoryId = model.CategoryId,
                SpecificationsJson = specsJson, // GÁN CHUỖI JSON
                DetailContentJson = detailJson  // GÁN CHUỖI JSON
            };

            _context.Add(product);
            await _context.SaveChangesAsync(); // Lưu lần 1 để Product có ProductId

            // --- BƯỚC 3: TẠO INVENTORY VÀ XỬ LÝ ẢNH ---

            // Tạo Inventory (Dùng Quantity từ ViewModel)
            var inventory = new Inventory
            {
                ProductId = product.ProductId,
                Quantity = model.Quantity
            };
            _context.Inventory.Add(inventory);
            await _context.SaveChangesAsync();

            // Xử lý ảnh chính (dùng model.Images)
            if (model.Images != null && model.Images.Count > 0)
            {
                var imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(imageFolder))
                {
                    Directory.CreateDirectory(imageFolder);
                }

                bool firstImage = true;
                foreach (var image in model.Images)
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

            // Chuyển hướng về trang Index của ProductsController cũ hoặc Details của sản phẩm mới tạo
            return RedirectToAction("Index", "Products");
        }

    }
}