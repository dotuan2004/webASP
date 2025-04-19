using LTW.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace LTW.Controllers
{
    public class HomeController : Controller
    {
        MyDataDataContext dt = new MyDataDataContext();


        public List<SanPham> SearchByName(string searchString)
        {
            var all_sach = (from ss in dt.SanPhams select ss).Where(m => m.SoLuongTon > 0 && m.TenSP.Contains(searchString)).ToList();
            return all_sach;
        }

        public ActionResult Index(string searchString, string sortOrder)
        {
            SetLikedProducts();
            ViewBag.Keyword = searchString;
            HomeModel Hm = new HomeModel();

            // Xử lý search
            if (searchString != null)
            {
                var searchResults = SearchByName(searchString);

                // Sort cho kết quả search
                switch (sortOrder)
                {
                    case "price_asc":
                        searchResults = searchResults.OrderBy(p => p.GiaSP).ToList();
                        break;
                    case "price_desc":
                        searchResults = searchResults.OrderByDescending(p => p.GiaSP).ToList();
                        break;
                    case "newest":
                        searchResults = searchResults.OrderByDescending(p => p.MaSP).ToList();
                        break;
                }

                return View(searchResults);
            }

            // Nếu không có search thì xử lý sort cho toàn bộ sản phẩm
            var allProducts = dt.SanPhams.AsQueryable();

            switch (sortOrder)
            {
                case "price_asc":
                    allProducts = allProducts.OrderBy(p => p.GiaSP);
                    break;
                case "price_desc":
                    allProducts = allProducts.OrderByDescending(p => p.GiaSP);
                    break;
                case "newest":
                    allProducts = allProducts.OrderByDescending(p => p.MaSP);
                    break;
            }

            Hm.listSP = allProducts.ToList();
            Hm.listLoai = dt.Loais.ToList();

            return View(Hm);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        private void SetLikedProducts()
        {
            var user = Session["TaiKhoan"] as KhachHang;
            if (user != null)
            {
                var likedProducts = dt.YeuThiches
                    .Where(y => y.MaKH == user.MaKH)
                    .Select(y => y.MaSP)
                    .ToList();
                ViewBag.LikedProducts = likedProducts;
            }
        }

        private decimal GetDiscountedPrice(SanPham sanPham)
        {
            // Lấy khuyến mãi đang active và có % giảm cao nhất
            var khuyenMai = dt.SanPham_KhuyenMais
                .Where(spkm => spkm.MaSP == sanPham.MaSP
                       && spkm.KhuyenMai.NgayBatDau <= DateTime.Now
                       && spkm.KhuyenMai.NgayKetThuc >= DateTime.Now
                       && spkm.KhuyenMai.TrangThai == true)
                .OrderByDescending(spkm => spkm.KhuyenMai.PhanTramGiam)
                .Select(spkm => spkm.KhuyenMai)
                .FirstOrDefault();

            if (khuyenMai != null)
            {
                return sanPham.GiaSP * (1 - ((decimal)khuyenMai.PhanTramGiam / 100));
            }

            return sanPham.GiaSP;
        }

        // Helper method lấy thông tin khuyến mãi
        private KhuyenMai GetActivePromotion(int maSP)
        {
            return dt.SanPham_KhuyenMais
                .Where(spkm => spkm.MaSP == maSP
                       && spkm.KhuyenMai.NgayBatDau <= DateTime.Now
                       && spkm.KhuyenMai.NgayKetThuc >= DateTime.Now
                       && spkm.KhuyenMai.TrangThai == true)
                .OrderByDescending(spkm => spkm.KhuyenMai.PhanTramGiam)
                .Select(spkm => spkm.KhuyenMai)
                .FirstOrDefault();
        }
    }
}