using LTW.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace LTW.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DonHangsController : Controller
    {
        MyDataDataContext data = new MyDataDataContext();

        // Thêm hàm tạo công nợ
        private void TaoCongNoTuDonHang(DonHang donHang)
        {
            // Chỉ tạo công nợ khi đơn hàng chưa thanh toán đủ
            if (donHang.TrangThai == "chuagiao") // hoặc điều kiện phù hợp với logic của bạn
            {
                var congNo = new CongNo
                {
                    MaKH = donHang.MaKH,
                    NgayPhatSinh = DateTime.Now,
                    SoTienNo = donHang.TongTienDonHang,
                    DaThanhToan = 0, // Có thể thay đổi tùy logic
                    ConLai = donHang.TongTienDonHang,
                    HanThanhToan = DateTime.Now.AddDays(30),
                    TrangThai = "Đang nợ"
                };

                data.CongNos.InsertOnSubmit(congNo);
                data.SubmitChanges();

                var chiTietCongNo = new ChiTietCongNo
                {
                    MaCN = congNo.MaCN,
                    MaDH = donHang.MaDH,
                    MaTT = donHang.MaTT,
                    SoTien = donHang.TongTienDonHang,
                    LoaiPhatSinh = "Nợ mới từ đơn hàng",
                    GhiChu = $"Công nợ từ đơn hàng {donHang.MaDH}"
                };

                data.ChiTietCongNos.InsertOnSubmit(chiTietCongNo);
                data.SubmitChanges();
            }
        }


        // Hiển thị danh sách đơn hàng
        public ActionResult ListDonHang(string searchString)
        {
            ViewBag.Keyword = searchString;

            var all_donhang = from dh in data.DonHangs
                              orderby dh.MaDH descending  // Thêm orderby để sắp xếp giảm dần
                              select dh;

            // Log ra toàn bộ đơn hàng
            System.Diagnostics.Debug.WriteLine("=== ALL ORDERS ===");
            foreach (var order in all_donhang)
            {
                System.Diagnostics.Debug.WriteLine(order.TrangThai);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                all_donhang = all_donhang.Where(dh => dh.MaDH.ToString().Contains(searchString) ||
                                                     dh.KhachHang.TenKhachHang.Contains(searchString))
                                        .OrderByDescending(dh => dh.MaDH); // Thêm OrderByDescending cho kết quả tìm kiếm

                // Log ra kết quả tìm kiếm
                System.Diagnostics.Debug.WriteLine($"=== SEARCH RESULTS FOR: {searchString} ===");
                foreach (var order in all_donhang)
                {
                    System.Diagnostics.Debug.WriteLine($"Found order - MaDH: {order.MaDH}, KhachHang: {order.KhachHang?.TenKhachHang}");
                }
            }

            return View(all_donhang);
        }

        // Xem chi tiết đơn hàng
        public ActionResult Detail(int id, int matt)
        {
            var donhang = data.DonHangs.FirstOrDefault(d => d.MaDH == id && d.MaTT == matt);
            if (donhang == null)
            {
                return HttpNotFound();
            }

            // Lấy chi tiết đơn hàng
            var chitiet = data.ChiTietDonHangs.Where(ct => ct.MaDH == id && ct.MaTT == matt);
            ViewBag.ChiTietDonHang = chitiet;

            return View(donhang);
        }

        // Cập nhật trạng thái đơn hàng
        [HttpPost]
        public ActionResult UpdateStatus(int id, int matt, string trangthai)
        {
            try
            {
                var donhang = data.DonHangs.FirstOrDefault(d => d.MaDH == id && d.MaTT == matt);
                if (donhang != null)
                {
                    string trangThaiCu = donhang.TrangThai;
                    donhang.TrangThai = trangthai;

                    // Xử lý công nợ khi trạng thái thay đổi
                    if (trangThaiCu == "chuagiao" && trangthai == "dagiao")
                    {
                        // Tạo công nợ nếu chưa có
                        var congNoHienTai = data.CongNos.FirstOrDefault(cn =>
                            data.ChiTietCongNos.Any(ct => ct.MaDH == id && ct.MaTT == matt && ct.MaCN == cn.MaCN));

                        if (congNoHienTai == null)
                        {
                            TaoCongNoTuDonHang(donhang);
                        }
                    }
                    else if (trangthai == "trahang")
                    {
                        // Xử lý công nợ khi trả hàng
                        var congNo = data.CongNos.FirstOrDefault(cn =>
                            data.ChiTietCongNos.Any(ct => ct.MaDH == id && ct.MaTT == matt && ct.MaCN == cn.MaCN));

                        if (congNo != null)
                        {
                            congNo.TrangThai = "Đã hủy - Trả hàng";
                            congNo.ConLai = 0;
                        }
                    }

                    data.SubmitChanges();
                    TempData["Message"] = "Cập nhật trạng thái thành công!";
                }
                return RedirectToAction("ListDonHang");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật!";
                return RedirectToAction("ListDonHang");
            }
        }

        // Xóa đơn hàng 
        [HttpPost]
        public ActionResult Delete(int id, int matt)
        {
            try
            {
                // Xóa chi tiết công nợ trước
                var chiTietCongNo = data.ChiTietCongNos.Where(ct => ct.MaDH == id && ct.MaTT == matt);
                if (chiTietCongNo.Any())
                {
                    data.ChiTietCongNos.DeleteAllOnSubmit(chiTietCongNo);
                }

                // Xóa công nợ nếu không còn chi tiết nào
                var congNos = data.CongNos.Where(cn =>
                    !data.ChiTietCongNos.Any(ct => ct.MaCN == cn.MaCN));
                if (congNos.Any())
                {
                    data.CongNos.DeleteAllOnSubmit(congNos);
                }

                // Xóa chi tiết đơn hàng
                var chitiet = data.ChiTietDonHangs.Where(ct => ct.MaDH == id && ct.MaTT == matt);
                if (chitiet.Any())
                {
                    data.ChiTietDonHangs.DeleteAllOnSubmit(chitiet);
                }

                // Xóa đơn hàng
                var donhang = data.DonHangs.FirstOrDefault(d => d.MaDH == id && d.MaTT == matt);
                if (donhang != null)
                {
                    data.DonHangs.DeleteOnSubmit(donhang);
                }

                data.SubmitChanges();
                TempData["Message"] = "Xóa đơn hàng thành công!";
                return RedirectToAction("ListDonHang");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa đơn hàng!";
                return RedirectToAction("ListDonHang");
            }
        }

        [HttpPost]
        public ActionResult UpdatePaymentStatus(int id, int matt, string trangthaithanhtoan)
        {
            try
            {
                var donhang = data.DonHangs.FirstOrDefault(d => d.MaDH == id && d.MaTT == matt);
                if (donhang != null)
                {
                    donhang.TrangThaiThanhToan = trangthaithanhtoan;

                    // Cập nhật công nợ khi thanh toán
                    if (trangthaithanhtoan == "dathanhtoan")
                    {
                        // Tìm công nợ liên quan
                        var congNo = (from cn in data.CongNos
                                      join ct in data.ChiTietCongNos
                                      on cn.MaCN equals ct.MaCN
                                      where ct.MaDH == id && ct.MaTT == matt
                                      select cn).FirstOrDefault();

                        if (congNo != null)
                        {
                            congNo.DaThanhToan = congNo.SoTienNo;
                            congNo.ConLai = 0;
                            congNo.TrangThai = "Đã thanh toán";
                        }
                    }

                    data.SubmitChanges();
                    TempData["Message"] = "Cập nhật trạng thái thanh toán thành công!";
                }
                return RedirectToAction("ListDonHang");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật!" + ex.Message;
                return RedirectToAction("ListDonHang");
            }
        }
    }
}