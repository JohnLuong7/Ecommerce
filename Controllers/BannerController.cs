/*using Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
// Đảm bảo thư viện này đã được thêm vào References trong dự án của bạn (thường là qua NuGet)
using System.Web.Mvc;

namespace Ecommerce.Controllers
{
    // Controller để quản lý các Banner
    public class BannerController : Controller
    {
        // Giả lập Database Context (Trong thực tế bạn sẽ dùng ApplicationDbContext hoặc DbContext của bạn)
        private static List<Banner> _mockBanners = new List<Banner>
        {
            new Banner { Id = 1, Title = "Giảm giá Mùa hè", ImageUrl = "/Images/banner1.jpg", DisplayOrder = 1, IsActive = true },
            new Banner { Id = 2, Title = "Sản phẩm Mới", ImageUrl = "/Images/banner2.jpg", DisplayOrder = 2, IsActive = true },
            new Banner { Id = 3, Title = "Đã Hết Hạn", ImageUrl = "/Images/banner3.jpg", DisplayOrder = 3, IsActive = false }
        };

        //
        // GET: /Banner/
        // Hiển thị danh sách tất cả các Banner
        public ActionResult Index()
        {
            // Trong thực tế: var banners = db.Banners.OrderBy(b => b.DisplayOrder).ToList();
            var banners = _mockBanners.OrderBy(b => b.DisplayOrder).ToList();
            return View(banners);
        }

        //
        // GET: /Banner/Create
        // Hiển thị form tạo Banner mới
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Banner/Create
        // Xử lý tạo Banner mới
        [HttpPost]
        [ValidateAntiForgeryToken] // Bảo vệ chống lại tấn công CSRF
        public ActionResult Create(Banner banner)
        {
            if (ModelState.IsValid)
            {
                // Thêm logic xử lý file upload ở đây nếu cần (ví dụ: Lưu file ảnh)

                // Trong thực tế: db.Banners.Add(banner); db.SaveChanges();
                banner.Id = _mockBanners.Max(b => b.Id) + 1; // Tạo ID giả
                _mockBanners.Add(banner);

                // Chuyển hướng về trang danh sách sau khi tạo thành công
                return RedirectToAction("Index");
            }

            // Nếu dữ liệu không hợp lệ, trả về form với dữ liệu đã nhập
            return View(banner);
        }

        //
        // GET: /Banner/Edit/5
        // Hiển thị form chỉnh sửa Banner
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                // HttpStatusCodeResult là lớp được định nghĩa trong System.Web.Mvc
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            // Trong thực tế: var banner = db.Banners.Find(id);
            var banner = _mockBanners.FirstOrDefault(b => b.Id == id);

            if (banner == null)
            {
                // HttpNotFound() là phương thức được kế thừa từ Controller
                return HttpNotFound();
            }
            return View(banner);
        }

        //
        // POST: /Banner/Edit/5
        // Xử lý cập nhật Banner
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Banner banner)
        {
            if (ModelState.IsValid)
            {
                // Trong thực tế: db.Entry(banner).State = EntityState.Modified; db.SaveChanges();
                var existingBanner = _mockBanners.FirstOrDefault(b => b.Id == banner.Id);
                if (existingBanner != null)
                {
                    existingBanner.Title = banner.Title;
                    existingBanner.ImageUrl = banner.ImageUrl;
                    existingBanner.DisplayOrder = banner.DisplayOrder;
                    existingBanner.IsActive = banner.IsActive;
                }

                return RedirectToAction("Index");
            }
            return View(banner);
        }

        //
        // GET: /Banner/Delete/5
        // Hiển thị xác nhận xóa
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            // Trong thực tế: var banner = db.Banners.Find(id);
            var banner = _mockBanners.FirstOrDefault(b => b.Id == id);
            if (banner == null)
            {
                return HttpNotFound();
            }
            return View(banner);
        }

        //
        // POST: /Banner/Delete/5
        // Xử lý xóa Banner
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // Trong thực tế: var banner = db.Banners.Find(id); db.Banners.Remove(banner); db.SaveChanges();
            var bannerToRemove = _mockBanners.FirstOrDefault(b => b.Id == id);
            if (bannerToRemove != null)
            {
                _mockBanners.Remove(bannerToRemove);
            }

            return RedirectToAction("Index");
        }

        // Phương thức hiển thị Banner trên trang chủ (Sử dụng cho Child Action)
        // Ví dụ: @Html.Action("ListBanners", "Banner")
        // ChildActionOnlyAttribute là lớp được định nghĩa trong System.Web.Mvc
        [ChildActionOnly]
        public PartialViewResult ListBanners()
        {
            // Chỉ lấy các banner đang hoạt động và sắp xếp theo thứ tự hiển thị
            var activeBanners = _mockBanners
                                .Where(b => b.IsActive)
                                .OrderBy(b => b.DisplayOrder)
                                .ToList();

            // Trả về một Partial View (Ví dụ: Views/Banner/ListBanners.cshtml)
            return PartialView("_BannerCarousel", activeBanners);
        }

        protected override void Dispose(bool disposing)
        {
            // Trong thực tế, nếu sử dụng DbContext, bạn sẽ Dispose DbContext ở đây.
            // if (disposing) { db.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
*/