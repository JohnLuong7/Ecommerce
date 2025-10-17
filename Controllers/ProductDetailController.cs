using Ecommerce.Data;
using Ecommerce.Models;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;

namespace Ecommerce.Controllers
{
    // Route mặc định: /ProductDetail
    public class ProductDetailController : Controller
    {
        private readonly AppDbContext _context;

        public ProductDetailController(AppDbContext context)
        {
            _context = context;
        }

        // =======================================================
        // GET: ProductDetail/Details/5 (Hiển thị chi tiết)
        // =======================================================
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // 1. Tải Sản phẩm và các mối quan hệ
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Inventory)
                // Tải ảnh và sắp xếp
                .Include(p => p.ProductImages.OrderBy(i => i.ProductImageId))
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            // 2. Xử lý/Giải mã JSON
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

            // 3. MAP ProductImages sang ExistingImageViewModel
            var existingImages = product.ProductImages
                .Select(i => new ExistingImageViewModel
                {
                    ImageId = i.ProductImageId,
                    ImageUrl = i.FileName,
                    IsPrimary = string.Equals(i.FileName, product.PrimaryImageUrl, StringComparison.OrdinalIgnoreCase)
                })
                .OrderByDescending(img => img.IsPrimary)
                .ThenBy(img => img.ImageId)
                .ToList();


            // 4. MAP PRODUCT MODEL SANG EditProductViewModel
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

                // Đính kèm dữ liệu JSON đã giải mã (List<T>)
                SpecificationGroups = specGroups,
                DetailContentBlocks = detailBlocks,

                // Đính kèm ảnh đã được Map
                ExistingImages = existingImages
            };

            // 5. Trả về View Details
            return View("~/Views/Products/Details.cshtml", model);
        }
    }
}