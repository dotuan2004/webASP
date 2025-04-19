using LTW.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LTW.Controllers
{
    public class GioHangController : Controller
    {
        // GET: GioHang
        MyDataDataContext data = new MyDataDataContext();

        public List<CartItem> LayGioHang()
        {
            // Nếu user đã đăng nhập
            var user = Session["TaiKhoan"] as KhachHang;
            if (user != null)
            {
                // Lấy từ database
                var dbCart = data.GioHangs
                    .Where(g => g.MaKH == user.MaKH && g.TrangThai == true)
                    .FirstOrDefault();

                if (dbCart != null)
                {
                    return data.ChiTietGioHangs
                        .Where(ct => ct.MaGioHang == dbCart.MaGioHang)
                        .Select(ct => new CartItem(ct))
                        .ToList();
                }
            }

            // Nếu chưa đăng nhập hoặc không có giỏ hàng trong DB, lấy từ Session
            List<CartItem> lstGioHang = Session["GioHang"] as List<CartItem>;
            if (lstGioHang == null)
            {
                lstGioHang = new List<CartItem>();
                Session["GioHang"] = lstGioHang;
            }
            return lstGioHang;
        }

        // Thêm vào giỏ hàng
        [HttpPost]
        public ActionResult ThemGioHang(int id, int soLuong = 1)
        {
            var user = Session["TaiKhoan"] as KhachHang;
            if (user != null)
            {
                // Thêm vào database
                ThemGioHangDatabase(user.MaKH, id, soLuong);
            }

            // Luôn cập nhật session để hiển thị ngay
            List<CartItem> lstGioHang = LayGioHang();
            CartItem sp = lstGioHang.Find(n => n.MaSP == id);

            if (sp == null)
            {
                sp = new CartItem(id);
                sp.isoluong = soLuong;
                lstGioHang.Add(sp);
            }
            else
            {
                sp.isoluong += soLuong;
            }

            return Json(new
            {
                success = true,
                message = "Thêm vào giỏ hàng thành công",
                totalItems = lstGioHang.Sum(x => x.isoluong)
            });
        }

        // Hàm hỗ trợ thêm vào database
        private void ThemGioHangDatabase(int maKH, int maSP, int soLuong)
        {
            var gioHang = data.GioHangs
                .Where(g => g.MaKH == maKH && g.TrangThai == true)
                .FirstOrDefault();

            if (gioHang == null)
            {
                gioHang = new Models.GioHang
                {
                    MaKH = maKH,
                    NgayTao = DateTime.Now,
                    TrangThai = true
                };
                data.GioHangs.InsertOnSubmit(gioHang);
                data.SubmitChanges();
            }

            var chiTiet = data.ChiTietGioHangs
                .Where(ct => ct.MaGioHang == gioHang.MaGioHang && ct.MaSP == maSP)
                .FirstOrDefault();

            if (chiTiet == null)
            {
                var sp = data.SanPhams.Single(s => s.MaSP == maSP);
                chiTiet = new ChiTietGioHang
                {
                    MaGioHang = gioHang.MaGioHang,
                    MaSP = maSP,
                    SoLuong = soLuong,
                    DonGia = sp.GiaSP,
                    ThanhTien = sp.GiaSP * soLuong
                };
                data.ChiTietGioHangs.InsertOnSubmit(chiTiet);
            }
            else
            {
                chiTiet.SoLuong += soLuong;
                chiTiet.ThanhTien = chiTiet.DonGia * chiTiet.SoLuong;
            }
            data.SubmitChanges();
        }

        // Merge giỏ hàng khi đăng nhập
        public void MergeCart()
        {
            var user = Session["TaiKhoan"] as KhachHang;
            if (user == null) return;

            var sessionCart = Session["GioHang"] as List<CartItem>;
            if (sessionCart != null && sessionCart.Any())
            {
                foreach (var item in sessionCart)
                {
                    ThemGioHangDatabase(user.MaKH, item.MaSP, item.isoluong);
                }
                Session["GioHang"] = null; // Xóa giỏ hàng session sau khi merge
            }
        }

        // Gọi hàm này sau khi đăng nhập thành công
        public ActionResult AfterLogin()
        {
            MergeCart();
            return RedirectToAction("GioHang");
        }

        // Tính tổng số lượng sản phẩm
        private int TongSoLuong()
        {
            List<CartItem> lstGioHang = LayGioHang(); // Sử dụng LayGioHang() thay vì trực tiếp từ Session
            return lstGioHang != null ? lstGioHang.Sum(n => n.isoluong) : 0;
        }

        // API trả về số lượng sản phẩm dạng JSON
        [HttpPost]
        public ActionResult TongSoLuongSanPhamJson()
        {
            List<CartItem> lstGioHang = LayGioHang();
            int tsl = lstGioHang != null ? lstGioHang.Count : 0;
            return Json(new { tsl = tsl });
        }

        // Đếm số loại sản phẩm trong giỏ
        private int TongSoLuongSanPham()
        {
            List<CartItem> lstGioHang = LayGioHang();
            return lstGioHang != null ? lstGioHang.Count : 0;
        }

        // Tính tổng tiền
        private double TongTien()
        {
            List<CartItem> lstGioHang = LayGioHang();
            return lstGioHang != null ? lstGioHang.Sum(n => n.dThanhtien) : 0;
        }

        public ActionResult GioHang()
        {
            List<CartItem> lstGioHang = LayGioHang();
            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();
            ViewBag.TongSoLuongSanPham = TongSoLuongSanPham();
            return View(lstGioHang);
        }

        public ActionResult GioHangPartial()
        {
            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();
            ViewBag.TongSoLuongSanPham = TongSoLuongSanPham();

            return PartialView();
        }

        public ActionResult XoaGioHang(int id)
        {
            var user = Session["TaiKhoan"] as KhachHang;
            if (user != null)
            {
                // Xóa từ database
                var dbCart = data.GioHangs
                    .FirstOrDefault(g => g.MaKH == user.MaKH && g.TrangThai == true);
                if (dbCart != null)
                {
                    var chiTiet = data.ChiTietGioHangs
                        .FirstOrDefault(ct => ct.MaGioHang == dbCart.MaGioHang && ct.MaSP == id);
                    if (chiTiet != null)
                    {
                        data.ChiTietGioHangs.DeleteOnSubmit(chiTiet);
                        data.SubmitChanges();
                    }
                }
            }

            // Xóa từ session
            List<CartItem> lstGioHang = LayGioHang();
            CartItem sanpham = lstGioHang.SingleOrDefault(n => n.MaSP == id);
            if (sanpham != null)
            {
                lstGioHang.RemoveAll(n => n.MaSP == id);
                TempData["SuccessMessage"] = $"Đã xóa sản phẩm {sanpham.TenSanPham} khỏi giỏ hàng!";
                return RedirectToAction("GioHang");
            }
            TempData["ErrorMessage"] = "Không tìm thấy sản phẩm cần xóa!";
            return RedirectToAction("GioHang");
        }

        public ActionResult CapNhatGioHang(int id, FormCollection collection)
        {
            var user = Session["TaiKhoan"] as KhachHang;
            int soLuongMoi = int.Parse(collection["txtSoLg"].ToString());

            if (user != null)
            {
                // Cập nhật database
                var dbCart = data.GioHangs
                    .FirstOrDefault(g => g.MaKH == user.MaKH && g.TrangThai == true);
                if (dbCart != null)
                {
                    var chiTiet = data.ChiTietGioHangs
                        .FirstOrDefault(ct => ct.MaGioHang == dbCart.MaGioHang && ct.MaSP == id);
                    if (chiTiet != null)
                    {
                        chiTiet.SoLuong = soLuongMoi;
                        chiTiet.ThanhTien = chiTiet.DonGia * soLuongMoi;
                        data.SubmitChanges();
                    }
                }
            }

            // Cập nhật session
            List<CartItem> lstGioHang = LayGioHang();
            CartItem sanpham = lstGioHang.SingleOrDefault(n => n.MaSP == id);
            if (sanpham != null)
            {
                sanpham.isoluong = soLuongMoi;
                TempData["SuccessMessage"] = $"Đã cập nhật số lượng sản phẩm {sanpham.TenSanPham} thành {soLuongMoi}!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm cần cập nhật!";
            }
            return RedirectToAction("GioHang");
        }

        public ActionResult XoaTatcaGioHang()
        {
            List<CartItem> lstGioHang = LayGioHang();
            if (lstGioHang.Any())
            {
                lstGioHang.Clear();
                TempData["SuccessMessage"] = "Đã xóa tất cả sản phẩm trong giỏ hàng!";
            }
            else
            {
                TempData["InfoMessage"] = "Giỏ hàng đã trống!";
            }
            return RedirectToAction("GioHang");
        }

        [HttpGet]
        public ActionResult DatHang()
        {
            if (Session["TaiKhoan"] == null)
                return RedirectToAction("DangNhap", "NguoiDung");

            var cart = LayGioHang();
            if (!cart.Any())
                return RedirectToAction("ListSanPham", "SanPham");

            decimal tongTien = (decimal)TongTien();
            var promotion = Session["AppliedPromotion"] as KhuyenMai;
            decimal giamGia = 0;

            if (promotion != null)
            {
                giamGia = tongTien * ((decimal)promotion.PhanTramGiam / 100);
            }

            ViewBag.TongTien = tongTien;
            ViewBag.DiscountAmount = giamGia;
            ViewBag.ShippingFee = 25000;
            ViewBag.FinalTotal = TongTienSauKhuyenMai();
            ViewBag.AppliedPromotion = promotion;
            ViewBag.TrangThai = data.ThanhToans.ToList();

            return View(cart);
        }

        [HttpPost]
        public ActionResult DatHang(FormCollection collection)
        {
            try
            {
                int status = int.Parse(collection["TrangThai"]);
                DonHang dh = new DonHang();
                KhachHang kh = (KhachHang)Session["TaiKhoan"];
                List<CartItem> gh = LayGioHang();

                // Kiểm tra đầu vào
                if (kh == null)
                {
                    return RedirectToAction("DangNhap", "NguoiDung");
                }

                if (!gh.Any())
                {
                    TempData["ErrorMessage"] = "Giỏ hàng trống!";
                    return RedirectToAction("GioHang");
                }

                var NgayGiaoHangDuKien = String.Format("{0:MM/dd/yyyy}", collection["NgayGiao"]);
                if (DateTime.Parse(NgayGiaoHangDuKien) < DateTime.Now)
                {
                    TempData["ErrorMessage"] = "Ngày giao hàng không hợp lệ!";
                    return RedirectToAction("DatHang");
                }

                // Tính toán các giá trị
                decimal tongTienHang = (decimal)TongTien();
                decimal chietKhau = 0; // Lấy từ form nếu có
                decimal phiVanChuyen = 25000; // Phí cố định hoặc lấy từ form
                decimal thueVAT = 10; // 10% VAT

                decimal thanhTienTruocVAT = tongTienHang - chietKhau + phiVanChuyen;
                decimal tienThueVAT = thanhTienTruocVAT * (thueVAT / 100);
                decimal thanhTienSauVAT = thanhTienTruocVAT + tienThueVAT;

                // Tạo đơn hàng với đầy đủ thông tin
                dh.MaKH = kh.MaKH;
                dh.NgayDatHang = DateTime.Now;
                dh.NgayGiaoHangDuKien = DateTime.Parse(NgayGiaoHangDuKien);
                dh.TrangThai = "chuagiao";
                dh.TrangThaiThanhToan = (status == 1) ? "chuathanhtoan" : "dathanhtoan";
                dh.MaTT = status;
                dh.TongTienDonHang = tongTienHang;
                dh.GhiChu = collection["GhiChu"]; // Nếu có trường ghi chú trong form
                dh.ChietKhau = chietKhau;
                dh.PhiVanChuyen = phiVanChuyen;
                dh.DiaChiGiao = kh.DiaChi; // Hoặc lấy từ form nếu có địa chỉ giao hàng khác
                dh.ThueVAT = thueVAT;
                dh.TienThueVAT = tienThueVAT;
                dh.ThanhTienTruocVAT = thanhTienTruocVAT;
                dh.ThanhTienSauVAT = thanhTienSauVAT;
                dh.NgayTao = DateTime.Now;

                data.DonHangs.InsertOnSubmit(dh);
                data.SubmitChanges();

                // Phần code xử lý chi tiết đơn hàng giữ nguyên
                foreach (var item in gh)
                {
                    var sp = data.SanPhams.SingleOrDefault(n => n.MaSP == item.MaSP);
                    if (sp == null)
                    {
                        TempData["ErrorMessage"] = $"Không tìm thấy sản phẩm có mã {item.MaSP}";
                        return RedirectToAction("DatHang");
                    }

                    if (sp.SoLuongTon < item.isoluong)
                    {
                        TempData["ErrorMessage"] = $"Sản phẩm {sp.TenSP} không đủ số lượng trong kho";
                        return RedirectToAction("DatHang");
                    }

                    ChiTietDonHang ctdh = new ChiTietDonHang
                    {
                        MaDH = dh.MaDH,
                        MaSP = item.MaSP,
                        MaTT = status,
                        SoLuongMua = item.isoluong,
                        TongTien = (decimal)item.dThanhtien
                    };

                    data.ChiTietDonHangs.InsertOnSubmit(ctdh);
                    sp.SoLuongTon -= item.isoluong;
                }

                // Xử lý xóa giỏ hàng
                var dbCart = data.GioHangs
                    .FirstOrDefault(g => g.MaKH == kh.MaKH && g.TrangThai == true);
                if (dbCart != null)
                {
                    var chiTietGioHang = data.ChiTietGioHangs
                        .Where(ct => ct.MaGioHang == dbCart.MaGioHang);
                    data.ChiTietGioHangs.DeleteAllOnSubmit(chiTietGioHang);
                    dbCart.TrangThai = false;
                }

                data.SubmitChanges();
                Session["GioHang"] = null;

                return RedirectToAction("XacNhanDonHang", "GioHang");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi đặt hàng: " + ex.Message);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đặt hàng. Vui lòng thử lại!";
                return RedirectToAction("DatHang");
            }
        }

        public ActionResult XacnhanDonHang()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ApplyPromoCode(string code)
        {
            try
            {
                var promotion = data.KhuyenMais.FirstOrDefault(km =>
                    km.MaKM.ToString() == code &&
                    km.NgayBatDau <= DateTime.Now &&
                    km.NgayKetThuc >= DateTime.Now &&
                    km.TrangThai == true);

                if (promotion == null)
                {
                    return Json(new { success = false, message = "Mã không hợp lệ hoặc đã hết hạn!" });
                }

                // Kiểm tra điều kiện áp dụng
                var cartTotal = (decimal)TongTien(); 
                if (promotion.DieuKienApDung.HasValue && cartTotal < promotion.DieuKienApDung.Value)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Đơn hàng cần tối thiểu {promotion.DieuKienApDung.Value:#,##0}₫ để áp dụng mã này!"
                    });
                }

                // Lưu khuyến mãi vào session
                Session["AppliedPromotion"] = promotion;

                return Json(new
                {
                    success = true,
                    message = $"Áp dụng mã giảm {promotion.PhanTramGiam}% thành công!"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại!" });
            }
        }

        // phương thức tính tổng tiền
        private decimal TongTienSauKhuyenMai()
        {
            decimal tongTien = (decimal)TongTien();
            var promotion = Session["AppliedPromotion"] as KhuyenMai;

            if (promotion != null)
            {
                decimal giamGia = tongTien * ((decimal)promotion.PhanTramGiam / 100);
                tongTien -= giamGia;
            }

            // Cộng phí vận chuyển
            tongTien += 25000; // Phí cố định 25k

            return tongTien;
        }
    }
}