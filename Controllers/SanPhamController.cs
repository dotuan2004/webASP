using LTW.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;

namespace LTW.Controllers
{
    public class SanPhamController : Controller
    {
        MyDataDataContext data = new MyDataDataContext();
        // GET: SanPham

        public List<SanPham> SearchByName(string searchString)
        {
            var all_sp = (from ss in data.SanPhams select ss).Where(m => m.SoLuongTon > 0 && m.TenSP.Contains(searchString)).ToList();
            return all_sp;
        }
        public ActionResult ListSanPham(int? page, string searchString)
        {
            SetLikedProducts();
            ViewBag.Keyword = searchString;
            if (page == null) page = 1;

            var all_sp = (from ss in data.SanPhams select ss)
                         .Where(m => m.SoLuongTon > 0)
                         .OrderBy(m => m.MaSP);

            int pageSize = 8;
            int pageNum = page ?? 1;

            IEnumerable<SanPham> resultList = all_sp;

            if (!string.IsNullOrEmpty(searchString))
            {
                resultList = SearchByName(searchString);
            }

            // Set ViewBag cho tất cả trường hợp
            ViewBag.PromotionPrices = resultList.ToDictionary(
                sp => sp.MaSP,
                sp => GetDiscountedPrice(sp)
            );

            ViewBag.Promotions = resultList.ToDictionary(
                sp => sp.MaSP,
                sp => GetActivePromotion(sp.MaSP)
            );

            return View(resultList.ToPagedList(pageNum, pageSize));
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(FormCollection collection, SanPham sp)
        {
            var E_TenSP = collection["TenSP"];
            var E_Hinh = "/Content/images/" + collection["Hinh"];
            var E_GiaSP = Convert.ToInt32(collection["GiaSP"]);
            var E_SoLuongTon = Convert.ToInt32(collection["SoLuongTon"]);
            var E_MoTa = collection["MoTa"];
            var E_MaLoai = Convert.ToInt32(collection["MaLoai"]);
            var E_NCC = Convert.ToInt32(collection["NCC"]); ;
            if (string.IsNullOrEmpty(E_TenSP))
            {
                ViewData["Error"] = "Don't empty!";
            }
            else
            {
                sp.TenSP = E_TenSP.ToString();
                sp.Hinh = E_Hinh.ToString();
                sp.GiaSP = E_GiaSP;
                sp.SoLuongTon = E_SoLuongTon;
                sp.MoTa = E_MoTa.ToString();
                sp.MaLoai = E_MaLoai;
                sp.MaNCC= E_NCC;
                data.SanPhams.InsertOnSubmit(sp);
                data.SubmitChanges();
                return RedirectToAction("ListSanPham");
            }
            return this.Create();
        }

        private void SetLikedProducts()
        {
            var user = Session["TaiKhoan"] as KhachHang;
            if (user != null)
            {
                var likedProducts = data.YeuThiches
                    .Where(y => y.MaKH == user.MaKH)
                    .Select(y => y.MaSP)
                    .ToList();
                ViewBag.LikedProducts = likedProducts;
            }
        }

        public ActionResult Detail(int id)
        {
            var product = data.SanPhams.FirstOrDefault(s => s.MaSP == id);
            if (product == null) return HttpNotFound();

            var user = Session["TaiKhoan"] as KhachHang;
            if (user != null)
            {
                ViewBag.IsLiked = data.YeuThiches.Any(y =>
                    y.MaKH == user.MaKH && y.MaSP == id);
            }

            // Lấy các sản phẩm cùng loại, ngoại trừ sản phẩm hiện tại
            var relatedProducts = data.SanPhams
                .Where(p => p.MaLoai == product.MaLoai && p.MaSP != id)
                .Take(10)
                .ToList();

            ViewBag.RelatedProducts = relatedProducts;
            return View(product);
        }

        public ActionResult Edit(int id)
        {
            var E_sp = data.SanPhams.First(m => m.MaSP == id);
            return View(E_sp);
        }

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {

            var E_SP = data.SanPhams.First(m => m.MaSP == id);
            var E_TenSP = collection["TenSP"];
            var E_Hinh = collection["Hinh"];
            var E_GiaSP = Convert.ToInt32(collection["GiaSP"]);
            var E_SoLuongTon = Convert.ToInt32(collection["SoLuongTon"]);
            var E_MoTa = collection["MoTa"];
            var E_MaLoai = Convert.ToInt32(collection["MaLoai"]);
            var E_NCC = Convert.ToInt32(collection["NCC"]); ;
            E_SP.MaSP = id;
            if (string.IsNullOrEmpty(E_TenSP))
            {
                ViewData["Error"] = "Don't empty!";
            }
            else
            {
                E_SP.TenSP = E_TenSP;
                E_SP.Hinh = E_Hinh;
                E_SP.GiaSP = E_GiaSP;
                E_SP.SoLuongTon = E_SoLuongTon;
                E_SP.MoTa = E_MoTa;
                E_SP.MaLoai = E_MaLoai;
                E_SP.MaNCC = E_NCC;
                UpdateModel(E_SP);
                data.SubmitChanges();
                return RedirectToAction("ListSanPham");

            }
            return this.Edit(id);

        }

        public string ProcessUpload(HttpPostedFileBase file)
        {
            if (file == null)
            {
                return "";
            }
            file.SaveAs(Server.MapPath("~/Content/images/" + file.FileName));
            return "/Content/images/" + file.FileName;
        }

        public ActionResult Delete(int id)
        {
            var D_sp = data.SanPhams.First(m => m.MaSP == id);
            return View(D_sp);
        }

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            var D_sp = data.SanPhams.Where(m => m.MaSP == id).FirstOrDefault();
            data.SanPhams.DeleteOnSubmit(D_sp);
            data.SubmitChanges();
            return RedirectToAction("ListSanPham");
        }

        // Controller action để thêm/xóa yêu thích
        [HttpPost]
        [Authorize]
        public ActionResult ToggleYeuThich(int maSP)
        {
            var user = Session["TaiKhoan"] as KhachHang;
            if (user == null) return RedirectToAction("DangNhap", "NguoiDung");

            var yeuThich = data.YeuThiches.FirstOrDefault(y =>
                y.MaKH == user.MaKH && y.MaSP == maSP);

            if (yeuThich != null)
            {
                // Xóa yêu thích nếu đã tồn tại
                data.YeuThiches.DeleteOnSubmit(yeuThich);
            }
            else
            {
                // Thêm mới nếu chưa tồn tại
                yeuThich = new YeuThich
                {
                    MaKH = user.MaKH,
                    MaSP = maSP,
                    NgayThem = DateTime.Now
                };
                data.YeuThiches.InsertOnSubmit(yeuThich);
            }

            data.SubmitChanges();
            return Json(new { success = true });
        }

        public ActionResult DanhSachYeuThich()
        {
            var user = Session["TaiKhoan"] as KhachHang;
            if (user == null)
                return RedirectToAction("DangNhap", "NguoiDung");

            var dsYeuThich = data.YeuThiches
                .Where(y => y.MaKH == user.MaKH)
                .Select(y => y.SanPham)
                .ToList();

            return View("YeuThich", dsYeuThich);
        }

        private decimal GetDiscountedPrice(SanPham sanPham)
        {
            // Lấy khuyến mãi đang active và có % giảm cao nhất
            var khuyenMai = data.SanPham_KhuyenMais
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
            return data.SanPham_KhuyenMais
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