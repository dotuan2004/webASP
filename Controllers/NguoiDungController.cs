using LTW.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Services.Description;
using System.Windows;

namespace LTW.Controllers
{
    public class NguoiDungController : Controller
    {
        // GET: NguoiDung
        MyDataDataContext data = new MyDataDataContext();

        public static bool ValidateVNPhoneNumber(string phoneNumber)
        {
            phoneNumber = phoneNumber.Replace("+84", "0");
            Regex regex = new
            Regex(@"^(0)(86|96|97|98|32|33|34|35|36|37|38|39|91|94|83|84|85|81|82|90|93|70|79|77|76|7
8|92|56|58|99|59|55|87)\d{7}$");
            return regex.IsMatch(phoneNumber);
        }

        public bool ValidateEmail(string email)
        {
            Regex regex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            return regex.IsMatch(email);
        }

        [HttpGet]
        public ActionResult DangKy()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Dangky(FormCollection collection, KhachHang kh)
        {
            var TenKhachHang = collection["TenKhachHang"];
            var UserName = collection["UserName"];
            var Password = collection["Password"];
            var MatKhauXacNhan = collection["MatKhauXacNhan"];
            var Email = collection["Email"];
            var DiaChi = collection["DiaChi"];
            var SDT = collection["SDT"];
            var RoleID = Convert.ToInt32(collection["RoleID"]);
            var check = data.KhachHangs.Where(n => n.UserName == UserName).Count();




            if (check > 0)
            {
                TempData["Error"] = "Ten dang nhap da ton tai!";
                return this.DangKy();
            }

            else if (String.IsNullOrEmpty(MatKhauXacNhan))
            {
                TempData["NhapMKXN"] = "Phải nhập mật khẩu xác nhận!";
            }

            else if (ValidateEmail(Email) == false)
            {
                TempData["Error"] = "Email khong hop le";
                return this.DangKy();

            }


            else if (ValidateVNPhoneNumber (SDT) == false)
            {
                TempData["Error"] = "So dien thoai ko hop le!";
                return this.DangKy();
            }
                       
            else
            {
                if (!Password.Equals(MatKhauXacNhan))
                {
                    TempData["MatKhauGiongNhau"] = "Mật khẩu và mật khẩu xác nhận phải giống nhau";
                }
                else
                {

                    kh.UserName = UserName;
                    
                    kh.Password = Password;
                    kh.TenKhachHang = TenKhachHang;
                    kh.Email = Email;
                    kh.DiaChi = DiaChi;
                    kh.SDT = SDT;
                    //kh.RoleID = RoleID;
                    kh.RoleID = 2;


                    data.KhachHangs.InsertOnSubmit(kh);
                    data.SubmitChanges();

                    return RedirectToAction("DangNhap");
                }

                
                

            }
            return this.DangKy();
        }

        [HttpGet]
        public ActionResult DangNhap()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DangNhap(FormCollection collection)
        {
            var UserName = collection["UserName"];
            var Password = collection["Password"];
            KhachHang kh = data.KhachHangs.SingleOrDefault(n => n.UserName.Equals(UserName) && n.Password.Equals(Password));

            if (kh != null)
            {
                // Đăng nhập thành công
                FormsAuthentication.SetAuthCookie(kh.UserName, false);
                Session["TaiKhoan"] = kh;

                // Merge giỏ hàng từ session vào database
                var sessionCart = Session["GioHang"] as List<CartItem>;
                if (sessionCart != null && sessionCart.Any())
                {
                    // Kiểm tra giỏ hàng trong database
                    var dbCart = data.GioHangs
                        .Where(g => g.MaKH == kh.MaKH && g.TrangThai == true)
                        .FirstOrDefault();

                    // Merge từng sản phẩm vào database
                    foreach (var item in sessionCart)
                    {
                        var ctgh = data.ChiTietGioHangs
                            .FirstOrDefault(ct => ct.MaGioHang == dbCart.MaGioHang && ct.MaSP == item.MaSP);

                        if (ctgh == null)
                        {
                            // Thêm mới nếu chưa có
                            ctgh = new ChiTietGioHang
                            {
                                MaGioHang = dbCart.MaGioHang,
                                MaSP = item.MaSP,
                                SoLuong = item.isoluong,
                                DonGia = (decimal)item.giaban,
                                ThanhTien = (decimal)item.dThanhtien,
                                NgayThem = DateTime.Now
                            };
                            data.ChiTietGioHangs.InsertOnSubmit(ctgh);
                        }
                        else
                        {
                            // Cộng số lượng nếu đã có
                            ctgh.SoLuong += item.isoluong;
                            ctgh.ThanhTien = ctgh.DonGia * ctgh.SoLuong;
                        }
                    }
                    data.SubmitChanges();

                    // Xóa giỏ hàng session sau khi đã merge
                    Session["GioHang"] = null;
                }

                // Chuyển hướng dựa vào role
                if (kh.RoleID == 1)
                {
                    return Redirect("/Admin/SanPhams/ListSanPham");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                TempData["Error"] = "Bạn nhập sai tài khoản hoặc mặc mật khẩu!";
            }
            return RedirectToAction("DangNhap");
        }

        public ActionResult DangXuat()
        {
            FormsAuthentication.SignOut();
            Session["TaiKhoan"] = null;  // Xóa thông tin khách hàng khỏi Session
            Session.Clear();             // Xóa tất cả các Session
            return RedirectToAction("DangNhap", "NguoiDung");
        }

        [Authorize]
        public ActionResult ThongTinCaNhan()
        {
            var user = Session["TaiKhoan"] as KhachHang;
            if (user == null) return RedirectToAction("DangNhap");

            return View(user);
        }

        [HttpPost]
        [Authorize]
        public ActionResult CapNhatThongTin(KhachHang model)
        {
            try
            {
                // Debug để xem dữ liệu
                System.Diagnostics.Debug.WriteLine($"MaKH: {model.MaKH}");
                System.Diagnostics.Debug.WriteLine($"TenKH: {model.TenKhachHang}");

                if (ModelState.IsValid)
                {
                    var khachHang = data.KhachHangs.SingleOrDefault(k => k.MaKH == model.MaKH);
                    if (khachHang != null)
                    {
                        // Cập nhật từng trường một
                        khachHang.TenKhachHang = model.TenKhachHang;
                        khachHang.Email = model.Email;
                        khachHang.SDT = model.SDT;
                        khachHang.DiaChi = model.DiaChi;

                        try
                        {
                            data.SubmitChanges();
                            Session["TaiKhoan"] = khachHang;  // Cập nhật lại session
                            TempData["Success"] = "Cập nhật thông tin thành công!";
                        }
                        catch (Exception ex)
                        {
                            // Debug lỗi
                            System.Diagnostics.Debug.WriteLine($"Error updating: {ex.Message}");
                            TempData["Error"] = "Có lỗi xảy ra khi cập nhật thông tin!";
                        }
                    }
                    else
                    {
                        TempData["Error"] = "Không tìm thấy thông tin khách hàng!";
                    }
                }
                else
                {
                    TempData["Error"] = "Dữ liệu không hợp lệ!";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra!";
            }

            return RedirectToAction("ThongTinCaNhan");
        }

        [HttpPost]
        [Authorize]
        public ActionResult DoiMatKhau(string currentPassword, string newPassword, string confirmPassword)
        {
            var user = Session["TaiKhoan"] as KhachHang;
            if (user == null) return RedirectToAction("DangNhap");

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Mật khẩu xác nhận không khớp!";
                return RedirectToAction("ThongTinCaNhan");
            }

            var khachHang = data.KhachHangs.FirstOrDefault(k => k.MaKH == user.MaKH);
            if (khachHang.Password != currentPassword)
            {
                TempData["Error"] = "Mật khẩu hiện tại không đúng!";
                return RedirectToAction("ThongTinCaNhan");
            }

            khachHang.Password = newPassword;
            data.SubmitChanges();
            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("ThongTinCaNhan");
        }

        [Authorize]
        public ActionResult LichSuDonHang()
        {
            var user = Session["TaiKhoan"] as KhachHang;
            if (user == null) return RedirectToAction("DangNhap");

            var donHang = data.DonHangs
                .Where(d => d.MaKH == user.MaKH)
                .OrderByDescending(d => d.NgayDatHang)
                .ToList();

            return View(donHang);
        }

        [Authorize]
        public ActionResult ChiTietDonHang(int id, int matt)
        {
            var donHang = data.DonHangs
                .FirstOrDefault(d => d.MaDH == id && d.MaTT == matt);

            if (donHang == null) return HttpNotFound();

            var chiTiet = data.ChiTietDonHangs
                .Where(ct => ct.MaDH == id && ct.MaTT == matt)
                .ToList();

            ViewBag.ChiTietDonHang = chiTiet;
            return PartialView("_ChiTietDonHang", donHang);
        }

        // Hàm hủy đơn hàng
        [HttpPost]
        [Authorize]
        public ActionResult HuyDonHang(int id, int matt)
        {
            try
            {
                var donHang = data.DonHangs
                    .FirstOrDefault(d => d.MaDH == id && d.MaTT == matt);

                // Debug thông tin
                System.Diagnostics.Debug.WriteLine($"MaDH: {id}, MaTT: {matt}");
                System.Diagnostics.Debug.WriteLine($"Đơn hàng null?: {donHang == null}");

                if (donHang != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Trạng thái: '{donHang.TrangThai}'");

                    // Kiểm tra điều kiện và trạng thái
                    if (donHang.TrangThai == "chuagiao")
                    {
                        try
                        {
                            // Cập nhật trạng thái
                            donHang.TrangThai = "dahuy";
                            data.SubmitChanges();

                            System.Diagnostics.Debug.WriteLine("Cập nhật thành công");
                            return Json(new { success = true, message = "Hủy đơn hàng thành công" });
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật: {ex.Message}");
                            return Json(new { success = false, message = $"Lỗi cập nhật: {ex.Message}" });
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "Đơn hàng không thể hủy do không ở trạng thái chờ giao" });
                    }
                }

                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi: {ex.Message}");
                return Json(new { success = false, message = $"Lỗi xử lý: {ex.Message}" });
            }
        }

        // Hàm trả hàng
        [HttpPost]
        [Authorize]
        public ActionResult TraHang(int id, int matt)
        {
            try
            {
                var donHang = data.DonHangs
                    .FirstOrDefault(d => d.MaDH == id && d.MaTT == matt);

                if (donHang != null && donHang.TrangThai == "dagiao")
                {
                    // Cập nhật số lượng tồn khi trả hàng
                    var chiTietDonHangs = data.ChiTietDonHangs
                        .Where(ct => ct.MaDH == id && ct.MaTT == matt);

                    foreach (var item in chiTietDonHangs)
                    {
                        var sanPham = data.SanPhams
                            .FirstOrDefault(sp => sp.MaSP == item.MaSP);
                        if (sanPham != null)
                        {
                            sanPham.SoLuongTon += item.SoLuongMua;
                        }
                    }

                    donHang.TrangThai = "trahang";
                    data.SubmitChanges();
                    return Json(new { success = true, message = "Đã xác nhận trả hàng thành công" });
                }

                return Json(new
                {
                    success = false,
                    message = "Không thể trả hàng do đơn hàng chưa được giao hoặc đã xử lý"
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xử lý trả hàng" });
            }
        }

        // Action hiển thị danh sách yêu thích
        [Authorize]
        public ActionResult DanhSachYeuThich()
        {
            var user = Session["TaiKhoan"] as KhachHang;
            if (user == null) return RedirectToAction("DangNhap", "NguoiDung");

            var dsYeuThich = data.YeuThiches
                .Where(y => y.MaKH == user.MaKH)
                .Select(y => y.SanPham)
                .ToList();

            return View(dsYeuThich);
        }
    }
}
