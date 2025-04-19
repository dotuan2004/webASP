using LTW.Areas.Admin.Model;
using LTW.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace LTW.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class PhieuNhapController : Controller
    {
        MyDataDataContext data = new MyDataDataContext();

        public ActionResult Create()
        {
            // Lấy danh sách nhà cung cấp
            var nccList = data.NCCs.ToList();
            ViewBag.MaNCC = new SelectList(nccList, "MaNCC", "TenNCC");

            // Lấy danh sách sản phẩm còn hoạt động
            var spList = data.SanPhams.Where(s => s.TrangThai == true).ToList();
            ViewBag.MaSP = new SelectList(spList, "MaSP", "TenSP", null);

            return View();
        }

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // Tạo phiếu nhập mới
                var phieuNhap = new PhieuNhap
                {
                    NgayNhap = DateTime.Now,
                    MaNCC = Convert.ToInt32(collection["MaNCC"]),
                    TongTien = 0, // Sẽ được cập nhật sau
                    GhiChu = collection["GhiChu"],
                    NguoiNhap = Convert.ToInt32(User.Identity.Name) // Assuming User.Identity.Name contains MaKH
                };

                data.PhieuNhaps.InsertOnSubmit(phieuNhap);
                data.SubmitChanges();

                // Lấy mã phiếu nhập vừa tạo
                int maPN = phieuNhap.MaPN;

                // Xử lý chi tiết phiếu nhập
                string[] maSPs = collection["MaSP"].Split(',');
                string[] soLuongs = collection["SoLuong"].Split(',');
                string[] donGias = collection["DonGia"].Split(',');

                decimal tongTien = 0;

                for (int i = 0; i < maSPs.Length; i++)
                {
                    if (!string.IsNullOrEmpty(maSPs[i]))
                    {
                        int maSP = Convert.ToInt32(maSPs[i]);
                        int soLuong = Convert.ToInt32(soLuongs[i]);
                        decimal donGia = Convert.ToDecimal(donGias[i]);
                        decimal thanhTien = soLuong * donGia;

                        var chiTietPN = new ChiTietPhieuNhap
                        {
                            MaPN = maPN,
                            MaSP = maSP,
                            SoLuong = soLuong,
                            DonGia = donGia,
                            ThanhTien = thanhTien
                        };

                        // Cập nhật số lượng tồn trong bảng SanPham
                        var sanPham = data.SanPhams.FirstOrDefault(sp => sp.MaSP == maSP);
                        if (sanPham != null)
                        {
                            sanPham.SoLuongTon += soLuong;
                        }

                        tongTien += thanhTien;
                        data.ChiTietPhieuNhaps.InsertOnSubmit(chiTietPN);
                    }
                }

                // Cập nhật tổng tiền phiếu nhập
                phieuNhap.TongTien = tongTien;
                data.SubmitChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi nhập kho: " + ex.Message;
                return View();
            }
        }

        // GET: Hiển thị danh sách phiếu nhập
        public ActionResult Index()
        {
            var phieuNhaps = from pn in data.PhieuNhaps
                             join ncc in data.NCCs on pn.MaNCC equals ncc.MaNCC
                             join kh in data.KhachHangs on pn.NguoiNhap equals kh.MaKH
                             select new PhieuNhapViewModel
                             {
                                 MaPN = pn.MaPN,
                                 NgayNhap = pn.NgayNhap,
                                 TenNCC = ncc.TenNCC,
                                 TongTien = pn.TongTien,
                                 NguoiNhap = kh.TenKhachHang,
                                 GhiChu = pn.GhiChu
                             };

            return View(phieuNhaps.ToList());
        }

        // GET: Chi tiết phiếu nhập
        public ActionResult Details(int id)
        {
            var chiTietPhieuNhap = from ct in data.ChiTietPhieuNhaps
                                   join sp in data.SanPhams on ct.MaSP equals sp.MaSP
                                   where ct.MaPN == id
                                   select new ChiTietPhieuNhapViewModel
                                   {
                                       MaCTPN = ct.MaCTPN,
                                       TenSP = sp.TenSP,
                                       SoLuong = ct.SoLuong,
                                       DonGia = ct.DonGia,
                                       ThanhTien = ct.ThanhTien
                                   };

            ViewBag.PhieuNhap = data.PhieuNhaps.FirstOrDefault(p => p.MaPN == id);
            return View(chiTietPhieuNhap.ToList());
        }
    }
}