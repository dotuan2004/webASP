using LTW.Areas.Admin;
using LTW.Models;
using System.Linq;
using System.Web.Mvc;

namespace LTW.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class AdminController : Controller
    {
        MyDataDataContext data = new MyDataDataContext();

        public ActionResult Search(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return RedirectToAction("Index", "Home", new { area = "Admin" });

            ViewBag.Keyword = keyword;

            // Tìm kiếm sản phẩm
            var products = data.SanPhams
                .Where(s => s.TenSP.Contains(keyword) ||
                           s.MoTa.Contains(keyword))
                .Take(5)
                .OrderByDescending(s => s.MaSP)
                .ToList();

            // Tìm kiếm đơn hàng
            var orders = data.DonHangs
                .Where(d => d.MaDH.ToString().Contains(keyword) ||
                           d.KhachHang.TenKhachHang.Contains(keyword))
                .Take(5)
                .OrderByDescending(d => d.MaDH)
                .ToList();

            // Tìm kiếm nhà cung cấp
            var suppliers = data.NCCs
                .Where(n => n.TenNCC.Contains(keyword) ||
                           n.SDT.Contains(keyword) ||
                           n.Email.Contains(keyword))
                .Take(5)
                .OrderByDescending(n => n.MaNCC)
                .ToList();

            ViewBag.Products = products;
            ViewBag.Orders = orders;
            ViewBag.Suppliers = suppliers;

            return View();
        }
    }
}