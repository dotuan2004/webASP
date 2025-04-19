using LTW.Areas.Admin.Model;
using LTW.Models;
using System.Web.Mvc;
using System;
using System.Linq;

namespace LTW.Areas.Admin.Controllers
{
    public class KhuyenMaiController : Controller
    {
        private MyDataDataContext data = new MyDataDataContext();

        // GET: Hiển thị danh sách khuyến mãi
        public ActionResult Index()
        {
            var khuyenMai = data.KhuyenMais.ToList();
            return View(khuyenMai);
        }

        // GET: Tạo khuyến mãi mới
        public ActionResult Create()
        {
            // Lấy danh sách sản phẩm chưa có khuyến mãi hoặc KM hết hạn
            var availableProducts = data.SanPhams
                .Where(sp => !sp.SanPham_KhuyenMais.Any(km =>
                    km.KhuyenMai.NgayKetThuc >= DateTime.Now))
                .ToList();

            ViewBag.Products = new MultiSelectList(availableProducts, "MaSP", "TenSP");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(KhuyenMai model, int[] selectedProducts)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Thêm khuyến mãi
                    data.KhuyenMais.InsertOnSubmit(model);
                    data.SubmitChanges();

                    // Thêm liên kết với sản phẩm
                    if (selectedProducts != null && selectedProducts.Length > 0)
                    {
                        foreach (var productId in selectedProducts)
                        {
                            var spkm = new SanPham_KhuyenMai
                            {
                                MaSP = productId,
                                MaKM = model.MaKM,
                                NgayApDung = DateTime.Now
                            };
                            data.SanPham_KhuyenMais.InsertOnSubmit(spkm);
                        }
                        data.SubmitChanges();
                    }

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi thêm khuyến mãi: " + ex.Message);
                }
            }

            // Nếu có lỗi, load lại danh sách sản phẩm
            var availableProducts = data.SanPhams.ToList();
            ViewBag.Products = new MultiSelectList(availableProducts, "MaSP", "TenSP");
            return View(model);
        }

        // GET: Chỉnh sửa khuyến mãi
        public ActionResult Edit(int id)
        {
            var khuyenMai = data.KhuyenMais.FirstOrDefault(km => km.MaKM == id);
            if (khuyenMai == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách sản phẩm đang áp dụng KM này
            var selectedProducts = khuyenMai.SanPham_KhuyenMais
                .Select(spkm => spkm.MaSP)
                .ToList();

            // Lấy tất cả sản phẩm có thể áp dụng
            var availableProducts = data.SanPhams.ToList();

            ViewBag.Products = new MultiSelectList(
                availableProducts, "MaSP", "TenSP", selectedProducts);

            return View(khuyenMai);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(KhuyenMai model, int[] selectedProducts)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var khuyenMai = data.KhuyenMais.FirstOrDefault(km => km.MaKM == model.MaKM);
                    if (khuyenMai != null)
                    {
                        // Cập nhật thông tin khuyến mãi
                        khuyenMai.TenKM = model.TenKM;
                        khuyenMai.MoTa = model.MoTa;
                        khuyenMai.NgayBatDau = model.NgayBatDau;
                        khuyenMai.NgayKetThuc = model.NgayKetThuc;
                        khuyenMai.PhanTramGiam = model.PhanTramGiam;
                        khuyenMai.DieuKienApDung = model.DieuKienApDung;

                        // Xóa các liên kết cũ
                        var existingLinks = data.SanPham_KhuyenMais
                            .Where(spkm => spkm.MaKM == model.MaKM);
                        data.SanPham_KhuyenMais.DeleteAllOnSubmit(existingLinks);

                        // Thêm các liên kết mới
                        if (selectedProducts != null)
                        {
                            foreach (var productId in selectedProducts)
                            {
                                var spkm = new SanPham_KhuyenMai
                                {
                                    MaSP = productId,
                                    MaKM = model.MaKM,
                                    NgayApDung = DateTime.Now
                                };
                                data.SanPham_KhuyenMais.InsertOnSubmit(spkm);
                            }
                        }

                        data.SubmitChanges();
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi cập nhật khuyến mãi: " + ex.Message);
                }
            }

            var availableProducts = data.SanPhams.ToList();
            ViewBag.Products = new MultiSelectList(availableProducts, "MaSP", "TenSP", selectedProducts);
            return View(model);
        }

        public ActionResult Report(DateTime? FromDate, DateTime? ToDate, string Status)
        {
            // Mặc định lấy báo cáo tháng hiện tại
            if (!FromDate.HasValue)
                FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (!ToDate.HasValue)
                ToDate = FromDate.Value.AddMonths(1).AddDays(-1);

            ViewBag.FromDate = FromDate.Value.ToString("yyyy-MM-dd");
            ViewBag.ToDate = ToDate.Value.ToString("yyyy-MM-dd");
            ViewBag.Status = Status;

            var report = new KhuyenMaiReport();

            // Lấy danh sách khuyến mãi trong khoảng thời gian
            var promotions = data.KhuyenMais.Where(km =>
                km.NgayBatDau <= ToDate &&
                km.NgayKetThuc >= FromDate);

            if (!string.IsNullOrEmpty(Status))
            {
                var now = DateTime.Now;
                switch (Status)
                {
                    case "active":
                        promotions = promotions.Where(km =>
                            km.NgayBatDau <= now &&
                            km.NgayKetThuc >= now);
                        break;
                    case "upcoming":
                        promotions = promotions.Where(km =>
                            km.NgayBatDau > now);
                        break;
                    case "ended":
                        promotions = promotions.Where(km =>
                            km.NgayKetThuc < now);
                        break;
                }
            }

            // Tính toán các chỉ số tổng quan
            report.TotalPromotions = promotions.Count();
            report.ActivePromotions = promotions.Count(km =>
                km.NgayBatDau <= DateTime.Now &&
                km.NgayKetThuc >= DateTime.Now);

            // Lấy chi tiết từng khuyến mãi
            foreach (var km in promotions)
            {
                var detail = new PromotionDetailReport
                {
                    MaKM = km.MaKM,
                    TenKM = km.TenKM,
                    SoSanPham = km.SanPham_KhuyenMais.Count(),
                    SoDonHang = GetPromotionOrderCount(km.MaKM, FromDate.Value, ToDate.Value),
                    DoanhSo = GetPromotionRevenue(km.MaKM, FromDate.Value, ToDate.Value),
                    TienGiam = GetPromotionDiscount(km.MaKM, FromDate.Value, ToDate.Value)
                };
                detail.ThucThu = detail.DoanhSo - detail.TienGiam;

                report.PromotionDetails.Add(detail);

                // Cập nhật tổng số
                report.TotalProducts += detail.SoSanPham;
                report.TotalOrders += detail.SoDonHang;
                report.TotalRevenue += detail.DoanhSo;
                report.TotalDiscount += detail.TienGiam;
                report.ActualRevenue += detail.ThucThu;

                // Thêm dữ liệu cho biểu đồ
                report.ChartLabels.Add(km.TenKM);
                report.ChartData.Add(detail.ThucThu);
            }

            // Dữ liệu cho biểu đồ tròn
            var categoryStats = (from spkm in data.SanPham_KhuyenMais
                                 join sp in data.SanPhams on spkm.MaSP equals sp.MaSP
                                 join l in data.Loais on sp.MaLoai equals l.MaLoai
                                 where spkm.KhuyenMai.NgayBatDau <= ToDate
                                     && spkm.KhuyenMai.NgayKetThuc >= FromDate
                                 group l by new { l.MaLoai, l.TenLoai } into g
                                 select new
                                 {
                                     Label = g.Key.TenLoai,
                                     Count = g.Count()
                                 }).ToList();

            report.PieLabels = categoryStats.Select(x => x.Label).ToList();
            report.PieData = categoryStats.Select(x => x.Count).ToList();

            return View(report);
        }

        private int GetPromotionOrderCount(int maKM, DateTime fromDate, DateTime toDate)
        {
            return (from ct in data.ChiTietDonHangs
                    join dh in data.DonHangs on ct.MaDH equals dh.MaDH
                    join sp in data.SanPhams on ct.MaSP equals sp.MaSP
                    join spkm in data.SanPham_KhuyenMais on sp.MaSP equals spkm.MaSP
                    where spkm.MaKM == maKM
                        && dh.NgayDatHang >= fromDate
                        && dh.NgayDatHang <= toDate
                    select ct.MaDH).Distinct().Count();
        }

        private decimal GetPromotionRevenue(int maKM, DateTime fromDate, DateTime toDate)
        {
            return (from ct in data.ChiTietDonHangs
                    join dh in data.DonHangs on ct.MaDH equals dh.MaDH
                    join sp in data.SanPhams on ct.MaSP equals sp.MaSP
                    join spkm in data.SanPham_KhuyenMais on sp.MaSP equals spkm.MaSP
                    where spkm.MaKM == maKM
                        && dh.NgayDatHang >= fromDate
                        && dh.NgayDatHang <= toDate
                    select ct.TongTien).Sum() ?? 0;
        }

        private decimal GetPromotionDiscount(int maKM, DateTime fromDate, DateTime toDate)
        {
            var km = data.KhuyenMais.FirstOrDefault(k => k.MaKM == maKM);
            if (km == null) return 0;

            return (from ct in data.ChiTietDonHangs
                    join dh in data.DonHangs on ct.MaDH equals dh.MaDH
                    join sp in data.SanPhams on ct.MaSP equals sp.MaSP
                    join spkm in data.SanPham_KhuyenMais on sp.MaSP equals spkm.MaSP
                    where spkm.MaKM == maKM
                        && dh.NgayDatHang >= fromDate
                        && dh.NgayDatHang <= toDate
                    select ct.TongTien * (decimal)(km.PhanTramGiam / 100)).Sum() ?? 0;
        }
    }
}
