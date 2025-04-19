using LTW.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace LTW.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class AccountSettingsController : Controller
    {
        private MyDataDataContext data = new MyDataDataContext();

        public ActionResult Index()
        {
            var user = Session["TaiKhoan"] as KhachHang;
            if (user == null) return RedirectToAction("DangNhap", "NguoiDung", new { area = "" });
            return View(user);
        }

        [HttpPost]
        public ActionResult UpdateProfile(KhachHang model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = data.KhachHangs.SingleOrDefault(k => k.MaKH == model.MaKH);
                    if (user != null)
                    {
                        // Validate email
                        if (!IsValidEmail(model.Email))
                        {
                            TempData["Error"] = "Email không hợp lệ";
                            return RedirectToAction("Index");
                        }

                        // Validate phone
                        if (!IsValidPhoneNumber(model.SDT))
                        {
                            TempData["Error"] = "Số điện thoại không hợp lệ";
                            return RedirectToAction("Index");
                        }

                        user.TenKhachHang = model.TenKhachHang;
                        user.Email = model.Email;
                        user.SDT = model.SDT;
                        user.DiaChi = model.DiaChi;

                        data.SubmitChanges();
                        Session["TaiKhoan"] = user;
                        TempData["Success"] = "Cập nhật thông tin thành công!";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var user = Session["TaiKhoan"] as KhachHang;
            if (user == null) return RedirectToAction("DangNhap", "NguoiDung", new { area = "" });

            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin!";
                return RedirectToAction("Index");
            }

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Mật khẩu xác nhận không khớp!";
                return RedirectToAction("Index");
            }

            var admin = data.KhachHangs.SingleOrDefault(k => k.MaKH == user.MaKH);
            if (admin.Password != currentPassword)
            {
                TempData["Error"] = "Mật khẩu hiện tại không đúng!";
                return RedirectToAction("Index");
            }

            admin.Password = newPassword;
            data.SubmitChanges();
            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Index");
        }

        private bool IsValidEmail(string email)
        {
            var regex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            return regex.IsMatch(email);
        }

        private bool IsValidPhoneNumber(string phone)
        {
            phone = phone.Replace("+84", "0");
            var regex = new Regex(@"^(0)(86|96|97|98|32|33|34|35|36|37|38|39|91|94|83|84|85|81|82|90|93|70|79|77|76|78|92|56|58|99|59|55|87)\d{7}$");
            return regex.IsMatch(phone);
        }
    }
}