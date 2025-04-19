using ClosedXML.Excel;
using LTW.Areas.Admin.Model;
using LTW.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace LTW.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class BaoCaoController : Controller
    {
        private MyDataDataContext data = new MyDataDataContext();

        #region Báo Cáo Doanh Thu
        public ActionResult DoanhThuNgay()
        {
            var today = DateTime.Today;
            try
            {
                var model = new DoanhThuNgayViewModel();

                // 1. Lấy dữ liệu tổng quan
                var doanhThuNgay = (from dt in data.DoanhThus
                                    where dt.NgayBaoCao == today
                                    select dt).FirstOrDefault();

                if (doanhThuNgay != null)
                {
                    model.SoDonHang = doanhThuNgay.SoDonHang;
                    model.DoanhThu = doanhThuNgay.TongDoanhThu;
                    model.TongChiPhi = doanhThuNgay.TongChiPhi;
                    model.LoiNhuan = doanhThuNgay.LoiNhuan;
                }

                // 2. Lấy dữ liệu biểu đồ
                var doanhThuTheoGio = from dh in data.DonHangs
                                      where dh.NgayDatHang.Date == today
                                         && dh.TrangThai == "dagiao"
                                      group dh by dh.NgayDatHang.Hour into g
                                      select new
                                      {
                                          Gio = g.Key,
                                          DoanhThu = g.Sum(x => x.TongTienDonHang)
                                      };

                var chartData = new decimal[12];
                foreach (var item in doanhThuTheoGio)
                {
                    var index = item.Gio / 2;
                    if (index < 12)
                    {
                        chartData[index] += item.DoanhThu;
                    }
                }

                model.ChartData = chartData;
                ViewBag.ChartData = chartData;  // Cho biểu đồ

                return View(model);
            }
            catch (Exception ex)
            {
                // Log lỗi
                return View(new DoanhThuNgayViewModel());
            }
        }

        public ActionResult DoanhThuThang(DateTime? selectedDate = null)
        {
            try
            {
                var date = selectedDate ?? DateTime.Today;
                var currentMonth = date.Month;
                var currentYear = date.Year;

                var model = new DoanhThuThangViewModel
                {
                    NgayBaoCao = date
                };

                // Lấy doanh thu tháng hiện tại
                var doanhThuThang = (from dh in data.DonHangs
                                     where dh.NgayDatHang.Month == currentMonth
                                           && dh.NgayDatHang.Year == currentYear
                                           && dh.TrangThai == "dagiao"
                                     join ct in data.ChiTietDonHangs on dh.MaDH equals ct.MaDH
                                     join sp in data.SanPhams on ct.MaSP equals sp.MaSP
                                     group new { dh, ct, sp } by 1 into g
                                     select new
                                     {
                                         SoDonHang = g.Select(x => x.dh.MaDH).Distinct().Count(),
                                         DoanhThu = g.Sum(x => x.dh.TongTienDonHang),
                                         SoKhachHang = g.Select(x => x.dh.MaKH).Distinct().Count(),
                                         TongChiPhi = g.Sum(x => x.ct.SoLuongMua * x.sp.GiaVon)
                                     }).FirstOrDefault();

                if (doanhThuThang != null)
                {
                    model.SoDonHang = doanhThuThang.SoDonHang;
                    model.DoanhThu = doanhThuThang.DoanhThu;
                    model.SoKhachHang = doanhThuThang.SoKhachHang;
                    model.TongChiPhi = doanhThuThang.TongChiPhi;
                    model.LoiNhuan = model.DoanhThu - model.TongChiPhi;
                    model.TrungBinhDonHang = model.SoDonHang > 0 ? model.DoanhThu / model.SoDonHang : 0;
                }

                // Dữ liệu chart theo ngày
                var doanhThuNgay = from dh in data.DonHangs
                                   where dh.NgayDatHang.Month == currentMonth
                                         && dh.NgayDatHang.Year == currentYear
                                         && dh.TrangThai == "dagiao"
                                   group dh by dh.NgayDatHang.Day into g
                                   select new { Ngay = g.Key, DoanhThu = g.Sum(x => x.TongTienDonHang) };

                var daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);
                model.ChartData = new decimal[daysInMonth];

                foreach (var item in doanhThuNgay)
                {
                    if (item.Ngay <= daysInMonth)
                    {
                        model.ChartData[item.Ngay - 1] = item.DoanhThu;
                    }
                }

                // Tính % tăng trưởng
                var lastMonth = date.AddMonths(-1);
                var doanhThuThangTruoc = (from dh in data.DonHangs
                                          where dh.NgayDatHang.Month == lastMonth.Month
                                                && dh.NgayDatHang.Year == lastMonth.Year
                                                && dh.TrangThai == "dagiao"
                                          group dh by 1 into g
                                          select new
                                          {
                                              SoDonHang = g.Count(),
                                              DoanhThu = g.Sum(x => x.TongTienDonHang),
                                              SoKhachHang = g.Select(x => x.MaKH).Distinct().Count()
                                          }).FirstOrDefault();

                if (doanhThuThangTruoc != null && doanhThuThang != null)
                {
                    model.PhanTramDonHang = doanhThuThangTruoc.SoDonHang > 0
                        ? ((model.SoDonHang - doanhThuThangTruoc.SoDonHang) * 100.0M / doanhThuThangTruoc.SoDonHang)
                        : 0;

                    model.PhanTramDoanhThu = doanhThuThangTruoc.DoanhThu > 0
                        ? ((model.DoanhThu - doanhThuThangTruoc.DoanhThu) * 100.0M / doanhThuThangTruoc.DoanhThu)
                        : 0;

                    model.PhanTramKhachHang = doanhThuThangTruoc.SoKhachHang > 0
                        ? ((model.SoKhachHang - doanhThuThangTruoc.SoKhachHang) * 100.0M / doanhThuThangTruoc.SoKhachHang)
                        : 0;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                // Log lỗi
                return View(new DoanhThuThangViewModel());
            }
        }

        public FileResult ExportDoanhThuThang(DateTime? selectedDate = null)
        {
            try
            {
                var date = selectedDate ?? DateTime.Today;
                var currentMonth = date.Month;
                var currentYear = date.Year;
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Doanh thu tháng");
                    // Thiết lập header
                    worksheet.Cell("A1").Value = "BÁO CÁO DOANH THU THÁNG " + date.ToString("MM/yyyy");
                    worksheet.Range("A1:E1").Merge().Style.Font.Bold = true;
                    worksheet.Cell("A2").Value = $"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm}";
                    worksheet.Range("A2:E2").Merge();
                    // Header cho bảng dữ liệu
                    var headerRow = worksheet.Row(4);
                    headerRow.Style.Font.Bold = true;
                    worksheet.Cell("A4").Value = "Ngày";
                    worksheet.Cell("B4").Value = "Số đơn hàng";
                    worksheet.Cell("C4").Value = "Doanh thu";
                    worksheet.Cell("D4").Value = "Chi phí";
                    worksheet.Cell("E4").Value = "Lợi nhuận";
                    // Lấy dữ liệu
                    var query = from dh in data.DonHangs
                                where dh.NgayDatHang.Month == currentMonth
                                      && dh.NgayDatHang.Year == currentYear
                                      && dh.TrangThai == "dagiao"
                                group dh by dh.NgayDatHang.Date into g
                                select new
                                {
                                    Ngay = g.Key,
                                    SoDonHang = g.Count(),
                                    DoanhThu = g.Sum(x => x.TongTienDonHang),
                                };
                    // Điền dữ liệu
                    var row = 5;
                    foreach (var item in query.OrderBy(x => x.Ngay))
                    {
                        worksheet.Cell($"A{row}").Value = item.Ngay.ToString("dd/MM/yyyy");
                        worksheet.Cell($"B{row}").Value = item.SoDonHang;
                        worksheet.Cell($"C{row}").Value = item.DoanhThu;
                        worksheet.Cell($"D{row}").Value = item.DoanhThu * 0.7M; // Chi phí ước tính
                        worksheet.Cell($"E{row}").Value = item.DoanhThu * 0.3M; // Lợi nhuận ước tính
                        row++;
                    }
                    // Format
                    var tableRange = worksheet.Range($"A4:E{row - 1}");
                    tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    // Format các cột số
                    worksheet.Range($"C5:E{row - 1}").Style.NumberFormat.Format = "#,##0";
                    // Tự động điều chỉnh độ rộng cột
                    worksheet.Columns().AdjustToContents();
                    // Tính tổng
                    worksheet.Cell($"A{row + 1}").Value = "Tổng cộng:";
                    worksheet.Cell($"B{row + 1}").FormulaA1 = $"=SUM(B5:B{row - 1})";
                    worksheet.Cell($"C{row + 1}").FormulaA1 = $"=SUM(C5:C{row - 1})";
                    worksheet.Cell($"D{row + 1}").FormulaA1 = $"=SUM(D5:D{row - 1})";
                    worksheet.Cell($"E{row + 1}").FormulaA1 = $"=SUM(E5:E{row - 1})";
                    worksheet.Range($"A{row + 1}:E{row + 1}").Style.Font.Bold = true;
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Flush();
                        return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"BaoCaoDoanhThu_{date:yyyyMM}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log lỗi
                return null;
            }
        }

        public ActionResult DoanhThuLoai(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var model = new DoanhThuLoaiViewModel();

                // Xử lý ngày tháng
                model.TuNgay = fromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                model.DenNgay = toDate ?? DateTime.Today;

                // Lấy dữ liệu doanh thu theo loại
                var query = from ctdh in data.ChiTietDonHangs
                            join sp in data.SanPhams on ctdh.MaSP equals sp.MaSP
                            join l in data.Loais on sp.MaLoai equals l.MaLoai
                            join dh in data.DonHangs on ctdh.MaDH equals dh.MaDH
                            where dh.TrangThai == "dagiao"
                                  && dh.NgayDatHang >= model.TuNgay
                                  && dh.NgayDatHang <= model.DenNgay
                            group new { ctdh, sp, l } by new { l.MaLoai, l.TenLoai } into g
                            select new
                            {
                                LoaiSP = g.Key.TenLoai,
                                SoLuongBan = g.Sum(x => x.ctdh.SoLuongMua),
                                DoanhThu = g.Sum(x => x.ctdh.TongTien ?? 0),
                                TongChiPhi = g.Sum(x => x.ctdh.SoLuongMua * x.sp.GiaVon)
                            };

                var result = query.ToList();

                // Tính toán tổng số liệu
                model.TongDoanhThu = result.Sum(x => x.DoanhThu);
                model.TongChiPhi = result.Sum(x => x.TongChiPhi);
                model.LoiNhuan = model.TongDoanhThu - model.TongChiPhi;

                // Chuyển đổi dữ liệu và tính các chỉ số phụ
                model.DoanhThuLoai = result.Select(x => new DoanhThuLoaiDetail
                {
                    LoaiSP = x.LoaiSP,
                    SoLuongBan = x.SoLuongBan,
                    DoanhThu = x.DoanhThu,
                    TongChiPhi = x.TongChiPhi,
                    LoiNhuan = x.DoanhThu - x.TongChiPhi,
                    TyTrong = model.TongDoanhThu > 0 ? (x.DoanhThu * 100M / model.TongDoanhThu) : 0,
                    TrungBinhSP = x.SoLuongBan > 0 ? (x.DoanhThu / x.SoLuongBan) : 0
                })
                .OrderByDescending(x => x.DoanhThu)
                .ToList();

                // Dữ liệu cho biểu đồ
                ViewBag.Labels = model.DoanhThuLoai.Select(x => x.LoaiSP).ToArray();
                ViewBag.PieData = model.DoanhThuLoai.Select(x => x.DoanhThu).ToArray();
                ViewBag.BarData = model.DoanhThuLoai.Select(x => x.SoLuongBan).ToArray();

                return View(model);
            }
            catch (Exception ex)
            {
                // Log lỗi
                return View(new DoanhThuLoaiViewModel());
            }
        }
        #endregion

        #region Báo Cáo Sản Phẩm
        public ActionResult TopSanPham(DateTime? fromDate, DateTime? toDate, int top = 10)
        {
            try
            {
                var model = new TopSanPhamViewModel
                {
                    TuNgay = fromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                    DenNgay = toDate ?? DateTime.Today,
                    Top = top
                };

                // Lấy dữ liệu top sản phẩm
                var query = from ctdh in data.ChiTietDonHangs
                            join sp in data.SanPhams on ctdh.MaSP equals sp.MaSP
                            join dh in data.DonHangs on ctdh.MaDH equals dh.MaDH
                            where dh.TrangThai == "dagiao"
                                  && dh.NgayDatHang >= model.TuNgay
                                  && dh.NgayDatHang <= model.DenNgay
                            group new { ctdh, sp } by new { sp.MaSP, sp.TenSP, sp.Hinh } into g
                            select new
                            {
                                g.Key.MaSP,
                                g.Key.TenSP,
                                g.Key.Hinh,
                                SoLuongBan = g.Sum(x => x.ctdh.SoLuongMua),
                                DoanhThu = g.Sum(x => x.ctdh.TongTien ?? 0)
                            };

                var result = query.ToList();

                // Tính tổng
                model.TongDoanhThu = result.Sum(x => x.DoanhThu);
                model.TongSoLuong = result.Sum(x => x.SoLuongBan);

                // Chuyển đổi và sắp xếp dữ liệu
                model.DanhSachSanPham = result
                    .Select(x => new TopSanPhamDetail
                    {
                        MaSP = x.MaSP,
                        TenSP = x.TenSP,
                        Hinh = x.Hinh,
                        SoLuongBan = x.SoLuongBan,
                        DoanhThu = x.DoanhThu,
                        TyTrong = model.TongDoanhThu > 0 ? (x.DoanhThu * 100M / model.TongDoanhThu) : 0
                    })
                    .OrderByDescending(x => x.DoanhThu)
                    .Take(top)
                    .ToList();

                // Dữ liệu cho biểu đồ
                ViewBag.Labels = model.DanhSachSanPham.Select(x => x.TenSP).ToArray();
                ViewBag.RevenueData = model.DanhSachSanPham.Select(x => x.DoanhThu).ToArray();
                ViewBag.QuantityData = model.DanhSachSanPham.Select(x => x.SoLuongBan).ToArray();

                return View(model);
            }
            catch (Exception ex)
            {
                // Log lỗi
                return View(new TopSanPhamViewModel());
            }
        }

        public ActionResult TonKho()
        {
            var query = (from sp in data.SanPhams
                         join l in data.Loais on sp.MaLoai equals l.MaLoai
                         select new
                         {
                             sp.MaSP,
                             sp.TenSP,
                             LoaiSP = l.TenLoai,
                             sp.SoLuongTon,
                             sp.GiaVon,
                             GiaTriTon = sp.SoLuongTon * sp.GiaVon
                         }).ToList();

            // Thống kê tổng quan
            ViewBag.TongSanPham = query.Count;
            ViewBag.TongTonKho = query.Sum(x => x.SoLuongTon);
            ViewBag.TongGiaTriTon = query.Sum(x => x.GiaTriTon);
            ViewBag.SanPhamGanHet = query.Count(x => x.SoLuongTon <= 10);

            // Thống kê theo loại
            var thongKeTheoLoai = query
                .GroupBy(x => x.LoaiSP)
                .Select(g => new
                {
                    LoaiSP = g.Key,
                    SoLuongTon = g.Sum(x => x.SoLuongTon),
                    GiaTriTon = g.Sum(x => x.GiaTriTon)
                })
                .OrderByDescending(x => x.GiaTriTon)
                .ToList();

            // Dữ liệu cho biểu đồ
            ViewBag.Labels = thongKeTheoLoai.Select(x => x.LoaiSP).ToArray();
            ViewBag.PieData = thongKeTheoLoai.Select(x => x.GiaTriTon).ToArray();
            ViewBag.BarData = thongKeTheoLoai.Select(x => x.SoLuongTon).ToArray();

            // Dữ liệu chi tiết
            ViewBag.TonKho = query.Select(x => new TonKhoViewModel
            {
                MaSP = x.MaSP,
                TenSP = x.TenSP,
                LoaiSP = x.LoaiSP,
                SoLuongTon = x.SoLuongTon,
                GiaVon = x.GiaVon,
                GiaTriTon = x.GiaTriTon
            }).OrderBy(x => x.SoLuongTon);

            return View();
        }

        public FileResult ExportTonKho()
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Tồn kho");

                    // Thiết lập header
                    worksheet.Cell("A1").Value = "BÁO CÁO TỒN KHO";
                    worksheet.Range("A1:G1").Merge().Style.Font.Bold = true;
                    worksheet.Cell("A2").Value = $"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm}";
                    worksheet.Range("A2:G2").Merge();

                    // Lấy dữ liệu trực tiếp từ database
                    var query = (from sp in data.SanPhams
                                 join l in data.Loais on sp.MaLoai equals l.MaLoai
                                 select new
                                 {
                                     sp.MaSP,
                                     sp.TenSP,
                                     LoaiSP = l.TenLoai,
                                     sp.SoLuongTon,
                                     sp.GiaVon,
                                     GiaTriTon = sp.SoLuongTon * sp.GiaVon
                                 }).ToList();

                    // Tổng quan
                    worksheet.Cell("A3").Value = "Tổng số sản phẩm:";
                    worksheet.Cell("B3").Value = query.Count;
                    worksheet.Cell("D3").Value = "Tổng giá trị tồn:";
                    worksheet.Cell("E3").Value = query.Sum(x => x.GiaTriTon);
                    worksheet.Range("A3:G3").Style.Font.Bold = true;

                    // Header cho bảng dữ liệu
                    var headerRow = worksheet.Row(5);
                    headerRow.Style.Font.Bold = true;
                    worksheet.Cell("A5").Value = "Mã SP";
                    worksheet.Cell("B5").Value = "Tên sản phẩm";
                    worksheet.Cell("C5").Value = "Loại SP";
                    worksheet.Cell("D5").Value = "Tồn kho";
                    worksheet.Cell("E5").Value = "Giá vốn";
                    worksheet.Cell("F5").Value = "Giá trị tồn";
                    worksheet.Cell("G5").Value = "Trạng thái";

                    // Điền dữ liệu
                    var row = 6;
                    foreach (var item in query)
                    {
                        var trangThai = item.SoLuongTon == 0 ? "Hết hàng" :
                                       item.SoLuongTon <= 10 ? "Sắp hết" : "Bình thường";

                        worksheet.Cell($"A{row}").Value = item.MaSP;
                        worksheet.Cell($"B{row}").Value = item.TenSP;
                        worksheet.Cell($"C{row}").Value = item.LoaiSP;
                        worksheet.Cell($"D{row}").Value = item.SoLuongTon;
                        worksheet.Cell($"E{row}").Value = item.GiaVon;
                        worksheet.Cell($"F{row}").Value = item.GiaTriTon;
                        worksheet.Cell($"G{row}").Value = trangThai;
                        row++;
                    }

                    // Format
                    var tableRange = worksheet.Range($"A5:G{row - 1}");
                    tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Format các cột số
                    worksheet.Range($"D6:F{row - 1}").Style.NumberFormat.Format = "#,##0";

                    // Tự động điều chỉnh độ rộng cột
                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Flush();
                        return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"BaoCaoTonKho_{DateTime.Now:yyyyMMdd}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log lỗi
                return null;
            }
        }

        public ActionResult SanPhamHetHang()
        {
            try
            {
                var model = new SanPhamHetHangViewModel();
                var today = DateTime.Today;
                var thirtyDaysAgo = today.AddDays(-30);

                // Lấy số lượng đã bán trong 30 ngày
                var banhang30ngay = from ctdh in data.ChiTietDonHangs
                                    join dh in data.DonHangs on ctdh.MaDH equals dh.MaDH
                                    where dh.NgayDatHang >= thirtyDaysAgo && dh.TrangThai == "dagiao"
                                    group ctdh by ctdh.MaSP into g
                                    select new
                                    {
                                        MaSP = g.Key,
                                        SoLuongBan = g.Sum(x => x.SoLuongMua)
                                    };

                // Lấy danh sách sản phẩm sắp hết
                var query = (from sp in data.SanPhams
                             join l in data.Loais on sp.MaLoai equals l.MaLoai
                             join ncc in data.NCCs on sp.MaNCC equals ncc.MaNCC
                             where sp.SoLuongTon <= 10 && sp.TrangThai == true
                             join bh in banhang30ngay on sp.MaSP equals bh.MaSP into soldItems
                             from sold in soldItems.DefaultIfEmpty()
                             select new SanPhamHetHangDetail
                             {
                                 MaSP = sp.MaSP,
                                 TenSP = sp.TenSP,
                                 LoaiSP = l.TenLoai,
                                 NhaCungCap = ncc.TenNCC,
                                 SoLuongTon = sp.SoLuongTon,
                                 DaBan30Ngay = sold != null ? sold.SoLuongBan : 0,
                                 GiaVon = sp.GiaVon,
                                 SoLuongCanNhap = sold != null ?
                                     (int)Math.Ceiling(sold.SoLuongBan * 1.5M) - sp.SoLuongTon :
                                     10 - sp.SoLuongTon
                             }).ToList();

                model.DanhSachSanPham = query.OrderBy(x => x.SoLuongTon).ToList();

                // Tính các thống kê
                model.SoLuongHetHang = query.Count(x => x.SoLuongTon == 0);
                model.SoLuongSapHet = query.Count(x => x.SoLuongTon > 0);
                model.GiaTriCanNhap = query.Sum(x => x.SoLuongCanNhap * x.GiaVon);

                // Thống kê theo loại cho biểu đồ
                model.ThongKeTheoLoai = query
                    .GroupBy(x => x.LoaiSP)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Dữ liệu cho biểu đồ
                ViewBag.Labels = model.ThongKeTheoLoai.Keys.ToArray();
                ViewBag.ChartData = model.ThongKeTheoLoai.Values.ToArray();

                return View(model);
            }
            catch (Exception ex)
            {
                // Log lỗi
                return View(new SanPhamHetHangViewModel());
            }
        }
        #endregion

        #region Báo Cáo Khách Hàng
        public ActionResult TopKhachHang(DateTime? fromDate, DateTime? toDate, int top = 10)
        {
            try
            {
                var model = new TopKhachHangViewModel
                {
                    TuNgay = fromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                    DenNgay = toDate ?? DateTime.Today,
                    Top = top
                };

                // Lấy dữ liệu khách hàng
                var query = from dh in data.DonHangs
                            join kh in data.KhachHangs on dh.MaKH equals kh.MaKH
                            where dh.TrangThai == "dagiao"
                                  && dh.NgayDatHang >= model.TuNgay
                                  && dh.NgayDatHang <= model.DenNgay
                            group dh by new { kh.MaKH, kh.TenKhachHang, kh.Email, kh.SDT } into g
                            select new
                            {
                                MaKH = g.Key.MaKH,
                                TenKH = g.Key.TenKhachHang,
                                Email = g.Key.Email,
                                SoDienThoai = g.Key.SDT,
                                SoDonHang = g.Count(),
                                TongGiaTri = g.Sum(x => x.TongTienDonHang),
                                MuaGanNhat = g.Max(x => x.NgayDatHang)
                            };

                var result = query.ToList();

                // Tính tổng số liệu
                model.TongDoanhThu = result.Sum(x => x.TongGiaTri);
                model.TongSoDonHang = result.Sum(x => x.SoDonHang);

                // Chọn top khách hàng và tính tỷ lệ
                model.DanhSachKhachHang = result
                    .Select(x => new TopKhachHangDetail
                    {
                        MaKH = x.MaKH,
                        TenKH = x.TenKH,
                        Email = x.Email,
                        SoDienThoai = x.SoDienThoai,
                        SoDonHang = x.SoDonHang,
                        TongGiaTri = x.TongGiaTri,
                        MuaGanNhat = x.MuaGanNhat,
                        TyLeDonHang = model.TongSoDonHang > 0 ? (x.SoDonHang * 100M / model.TongSoDonHang) : 0,
                        TyLeDoanhThu = model.TongDoanhThu > 0 ? (x.TongGiaTri * 100M / model.TongDoanhThu) : 0
                    })
                    .OrderByDescending(x => x.TongGiaTri)
                    .Take(top)
                    .ToList();

                // Dữ liệu cho biểu đồ
                ViewBag.Labels = model.DanhSachKhachHang.Select(x => x.TenKH).ToArray();
                ViewBag.PieData = model.DanhSachKhachHang.Select(x => x.TongGiaTri).ToArray();
                ViewBag.BarData = model.DanhSachKhachHang.Select(x => x.SoDonHang).ToArray();

                return View(model);
            }
            catch (Exception ex)
            {
                // Log lỗi
                return View(new TopKhachHangViewModel());
            }
        }

        public ActionResult CongNo()
        {
            try
            {
                var model = new CongNoReportViewModel();
                var today = DateTime.Today;

                // Lấy dữ liệu công nợ
                var query = from cn in data.CongNos
                            join kh in data.KhachHangs on cn.MaKH equals kh.MaKH
                            select new CongNoKhachHangViewModel
                            {
                                MaKH = kh.MaKH,
                                TenKH = kh.TenKhachHang,
                                SoDienThoai = kh.SDT,
                                TongNo = cn.SoTienNo,
                                DaThanhToan = cn.DaThanhToan,
                                ConLai = cn.ConLai,
                                HanThanhToan = cn.HanThanhToan ?? today,
                                NgayQuaHan = cn.HanThanhToan.HasValue && cn.ConLai > 0 ?
                                    Math.Max(0, (today - cn.HanThanhToan.Value).Days) : 0
                            };

                model.DanhSachCongNo = query.OrderByDescending(x => x.NgayQuaHan).ToList();

                // Tính các thống kê tổng quan
                model.TongCongNo = model.DanhSachCongNo.Sum(x => x.TongNo);
                model.SoKhachHangNo = model.DanhSachCongNo.Count(x => x.ConLai > 0);
                model.NoQuaHan = model.DanhSachCongNo.Where(x => x.NgayQuaHan > 0).Sum(x => x.ConLai);
                model.DaThu = model.DanhSachCongNo.Sum(x => x.DaThanhToan);

                // Tính phân bố tuổi nợ
                model.TuoiNoData = new decimal[5];
                foreach (var item in model.DanhSachCongNo.Where(x => x.ConLai > 0))
                {
                    if (item.NgayQuaHan == 0)
                        model.TuoiNoData[0] += item.ConLai;  // Trong hạn
                    else if (item.NgayQuaHan <= 30)
                        model.TuoiNoData[1] += item.ConLai;  // 1-30 ngày
                    else if (item.NgayQuaHan <= 60)
                        model.TuoiNoData[2] += item.ConLai;  // 31-60 ngày
                    else if (item.NgayQuaHan <= 90)
                        model.TuoiNoData[3] += item.ConLai;  // 61-90 ngày
                    else
                        model.TuoiNoData[4] += item.ConLai;  // Trên 90 ngày
                }

                // Tính công nợ theo tháng
                model.CongNoTheoThang = Enumerable.Range(0, 6)
                    .Select(i => today.AddMonths(-i))
                    .ToDictionary(
                        month => month.ToString("MM/yyyy"),
                        month => model.DanhSachCongNo
                            .Where(x => x.HanThanhToan.Month == month.Month &&
                                       x.HanThanhToan.Year == month.Year)
                            .Sum(x => x.TongNo)
                    );

                // Dữ liệu cho biểu đồ
                ViewBag.Labels = model.CongNoTheoThang.Keys.Reverse().ToArray();
                ViewBag.BarData = model.CongNoTheoThang.Values.Reverse().ToArray();
                ViewBag.PieData = model.TuoiNoData;

                return View(model);
            }
            catch (Exception ex)
            {
                // Log lỗi
                return View(new CongNoReportViewModel());
            }
        }

        public FileResult ExportCongNo(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                if (!fromDate.HasValue)
                    fromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                if (!toDate.HasValue)
                    toDate = DateTime.Today;

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Công nợ khách hàng");

                    // Thiết lập header
                    worksheet.Cell("A1").Value = "BÁO CÁO CÔNG NỢ";
                    worksheet.Range("A1:I1").Merge().Style.Font.Bold = true;
                    worksheet.Cell("A2").Value = $"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm}";
                    worksheet.Range("A2:I2").Merge();

                    // Header cho bảng dữ liệu
                    var headerRow = worksheet.Row(4);
                    headerRow.Style.Font.Bold = true;
                    worksheet.Cell("A4").Value = "Mã KH";
                    worksheet.Cell("B4").Value = "Tên khách hàng";
                    worksheet.Cell("C4").Value = "Số điện thoại";
                    worksheet.Cell("D4").Value = "Tổng nợ";
                    worksheet.Cell("E4").Value = "Đã thanh toán";
                    worksheet.Cell("F4").Value = "Còn lại";
                    worksheet.Cell("G4").Value = "Hạn thanh toán";
                    worksheet.Cell("H4").Value = "Số ngày quá hạn";
                    worksheet.Cell("I4").Value = "Trạng thái";

                    // Lấy dữ liệu
                    var query = from cn in data.CongNos
                                join kh in data.KhachHangs on cn.MaKH equals kh.MaKH
                                select new
                                {
                                    kh.MaKH,
                                    TenKH = kh.TenKhachHang,
                                    SoDienThoai = kh.SDT,
                                    TongNo = cn.SoTienNo,
                                    DaThanhToan = cn.DaThanhToan,
                                    ConLai = cn.ConLai,
                                    HanThanhToan = cn.HanThanhToan,
                                    NgayQuaHan = cn.HanThanhToan.HasValue && DateTime.Now > cn.HanThanhToan.Value ?
                                        (DateTime.Now - cn.HanThanhToan.Value).Days : 0
                                };

                    // Điền dữ liệu
                    var row = 5;
                    foreach (var item in query)
                    {
                        var trangThai = item.ConLai == 0 ? "Đã thanh toán" :
                                       (item.NgayQuaHan > 0 ? "Quá hạn" : "Trong hạn");

                        worksheet.Cell($"A{row}").Value = item.MaKH;
                        worksheet.Cell($"B{row}").Value = item.TenKH;
                        worksheet.Cell($"C{row}").Value = item.SoDienThoai;
                        worksheet.Cell($"D{row}").Value = item.TongNo;
                        worksheet.Cell($"E{row}").Value = item.DaThanhToan;
                        worksheet.Cell($"F{row}").Value = item.ConLai;
                        worksheet.Cell($"G{row}").Value = item.HanThanhToan?.ToString("dd/MM/yyyy");
                        worksheet.Cell($"H{row}").Value = item.NgayQuaHan > 0 ? item.NgayQuaHan : 0;
                        worksheet.Cell($"I{row}").Value = trangThai;
                        row++;
                    }

                    // Format
                    var tableRange = worksheet.Range($"A4:I{row - 1}");
                    tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Format các cột số
                    worksheet.Range($"D5:F{row - 1}").Style.NumberFormat.Format = "#,##0";

                    // Tự động điều chỉnh độ rộng cột
                    worksheet.Columns().AdjustToContents();

                    // Tính tổng
                    worksheet.Cell($"B{row + 1}").Value = "Tổng cộng:";
                    worksheet.Cell($"D{row + 1}").FormulaA1 = $"=SUM(D5:D{row - 1})";
                    worksheet.Cell($"E{row + 1}").FormulaA1 = $"=SUM(E5:E{row - 1})";
                    worksheet.Cell($"F{row + 1}").FormulaA1 = $"=SUM(F5:F{row - 1})";
                    worksheet.Range($"B{row + 1}:I{row + 1}").Style.Font.Bold = true;

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Flush();
                        return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"BaoCaoCongNo_{DateTime.Now:yyyyMMdd}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log lỗi
                return null;
            }
        }
        #endregion

        #region Nhật Ký & Theo Dõi
        public ActionResult NhatKyBanHang(DateTime? fromDate, DateTime? toDate, string trangThai = "")
        {
            try
            {
                var model = new NhatKyBanHangViewModel
                {
                    TuNgay = fromDate ?? DateTime.Today.AddDays(-6),
                    DenNgay = toDate ?? DateTime.Today,
                    TrangThai = trangThai
                };

                // Truy vấn dữ liệu
                var query = from nk in data.NhatKyBanHangs
                            join sp in data.SanPhams on nk.MaSP equals sp.MaSP
                            join dh in data.DonHangs on nk.MaDH equals dh.MaDH
                            join tt in data.ThanhToans on nk.MaTT equals tt.MaTT
                            where nk.NgayGiaoDich >= model.TuNgay
                               && nk.NgayGiaoDich <= model.DenNgay
                            select new NhatKyBanHangDetailViewModel
                            {
                                NgayGiaoDich = nk.NgayGiaoDich ?? DateTime.Now,
                                MaDH = nk.MaDH,
                                TenSP = sp.TenSP,
                                SoLuong = nk.SoLuong,
                                DonGia = nk.DonGia,
                                ThanhTien = nk.ThanhTien,
                                ThanhToan = tt.TenTT,
                                TrangThai = nk.TrangThai
                            };

                // Lọc theo trạng thái
                if (!string.IsNullOrEmpty(trangThai))
                {
                    query = query.Where(x => x.TrangThai == trangThai);
                }

                model.DanhSachNhatKy = query.OrderByDescending(x => x.NgayGiaoDich).ToList();

                // Tính thống kê
                model.ThongKe = new ThongKeNhatKyViewModel
                {
                    SoDonHang = model.DanhSachNhatKy.Select(x => x.MaDH).Distinct().Count(),
                    DoanhThu = model.DanhSachNhatKy.Sum(x => x.ThanhTien),
                    SoSanPham = model.DanhSachNhatKy.Sum(x => x.SoLuong)
                };

                // Tính doanh thu theo ngày
                model.DoanhThuNgay = model.DanhSachNhatKy
                    .GroupBy(x => x.NgayGiaoDich.Date)
                    .OrderBy(g => g.Key)
                    .ToDictionary(
                        g => g.Key.ToString("dd/MM"),
                        g => g.Sum(x => x.ThanhTien)
                    );

                // Dữ liệu cho biểu đồ
                ViewBag.Labels = model.DoanhThuNgay.Keys.ToArray();
                ViewBag.ChartData = model.DoanhThuNgay.Values.ToArray();
                ViewBag.MaxDoanhThu = model.DoanhThuNgay.Any() ?
                    model.DoanhThuNgay.Values.Max() * 1.2M : 1000000;

                return View(model);
            }
            catch (Exception ex)
            {
                // Log lỗi
                return View(new NhatKyBanHangViewModel());
            }
        }

        public FileResult ExportNhatKyBanHang(DateTime? fromDate, DateTime? toDate, string trangThai = "")
        {
            Debug.WriteLine($"fromDate: {fromDate}");
            Debug.WriteLine($"toDate: {toDate}");
            Debug.WriteLine($"trangThai: {trangThai}");
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Nhật ký bán hàng");

                    // Thiết lập header
                    worksheet.Cell("A1").Value = "NHẬT KÝ BÁN HÀNG";
                    worksheet.Range("A1:H1").Merge().Style.Font.Bold = true;
                    worksheet.Cell("A2").Value = $"Từ ngày: {fromDate:dd/MM/yyyy} đến ngày: {toDate:dd/MM/yyyy}";
                    worksheet.Range("A2:H2").Merge();

                    // Header cho bảng dữ liệu
                    var headerRow = worksheet.Row(4);
                    headerRow.Style.Font.Bold = true;
                    worksheet.Cell("A4").Value = "Ngày";
                    worksheet.Cell("B4").Value = "Mã ĐH";
                    worksheet.Cell("C4").Value = "Sản phẩm";
                    worksheet.Cell("D4").Value = "Số lượng";
                    worksheet.Cell("E4").Value = "Đơn giá";
                    worksheet.Cell("F4").Value = "Thành tiền";
                    worksheet.Cell("G4").Value = "Thanh toán";
                    worksheet.Cell("H4").Value = "Trạng thái";

                    // Lấy dữ liệu và điền vào excel
                    var query = from nk in data.NhatKyBanHangs
                                join sp in data.SanPhams on nk.MaSP equals sp.MaSP
                                join dh in data.DonHangs on nk.MaDH equals dh.MaDH
                                join tt in data.ThanhToans on nk.MaTT equals tt.MaTT
                                where nk.NgayGiaoDich >= fromDate
                                   && nk.NgayGiaoDich <= toDate
                                   && (string.IsNullOrEmpty(trangThai) || nk.TrangThai == trangThai)
                                orderby nk.NgayGiaoDich descending
                                select new
                                {
                                    NgayGiaoDich = nk.NgayGiaoDich ?? DateTime.Now,
                                    nk.MaDH,
                                    TenSP = sp.TenSP,
                                    SoLuong = nk.SoLuong,
                                    DonGia = nk.DonGia,
                                    ThanhTien = nk.ThanhTien,
                                    ThanhToan = tt.TenTT,
                                    nk.TrangThai
                                };
                    // Thêm đoạn code debug này trước vòng lặp foreach
                    var checkData = query.ToList();
                    System.Diagnostics.Debug.WriteLine($"Số lượng bản ghi: {checkData.Count}");

                    var row = 5;
                    foreach (var item in query)
                    {
                        worksheet.Cell($"A{row}").Value = item.NgayGiaoDich.ToString("dd/MM/yyyy HH:mm");
                        worksheet.Cell($"B{row}").Value = item.MaDH;
                        worksheet.Cell($"C{row}").Value = item.TenSP;
                        worksheet.Cell($"D{row}").Value = item.SoLuong;
                        worksheet.Cell($"E{row}").Value = item.DonGia;
                        worksheet.Cell($"F{row}").Value = item.ThanhTien;
                        worksheet.Cell($"G{row}").Value = item.ThanhToan;
                        worksheet.Cell($"H{row}").Value = item.TrangThai;
                        row++;
                    }

                    // Format
                    worksheet.Columns().AdjustToContents();
                    worksheet.Range($"D5:F{row - 1}").Style.NumberFormat.Format = "#,##0";

                    // Lưu file
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Flush();
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"NhatKyBanHang_{DateTime.Now:yyyyMMdd}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log chi tiết lỗi
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner: {ex.InnerException.Message}");
                }
                return null;
            }
        }

        public ActionResult TheoDoiDonHang(DateTime? fromDate, DateTime? toDate, string trangThai = "")
        {
            try
            {
                var model = new TheoDoiDonHangViewModel
                {
                    TuNgay = fromDate ?? DateTime.Today.AddDays(-6),
                    DenNgay = toDate ?? DateTime.Today,
                    TrangThai = trangThai
                };

                // Lấy dữ liệu đơn hàng
                var query = from dh in data.DonHangs
                            join kh in data.KhachHangs on dh.MaKH equals kh.MaKH
                            join tt in data.ThanhToans on dh.MaTT equals tt.MaTT
                            where dh.NgayDatHang >= model.TuNgay && dh.NgayDatHang <= model.DenNgay
                            select new DonHangViewModel
                            {
                                MaDH = dh.MaDH,
                                NgayDat = dh.NgayDatHang,
                                KhachHang = kh.TenKhachHang,
                                SoDienThoai = kh.SDT,
                                TongTien = dh.TongTienDonHang,
                                ThanhToan = tt.TenTT,
                                NgayGiao = dh.NgayGiaoHangDuKien,
                                TrangThai = dh.TrangThai
                            };

                // Lọc theo trạng thái
                if (!string.IsNullOrEmpty(trangThai))
                {
                    query = query.Where(x => x.TrangThai == trangThai);
                }

                model.DanhSachDonHang = query.OrderByDescending(x => x.NgayDat).ToList();

                // Tính thống kê
                model.ThongKe = new ThongKeDonHangViewModel
                {
                    TongDon = model.DanhSachDonHang.Count,
                    DangXuLy = model.DanhSachDonHang.Count(x => x.TrangThai == "Đang xử lý"),
                    DangGiao = model.DanhSachDonHang.Count(x => x.TrangThai == "Đang giao"),
                    HoanThanh = model.DanhSachDonHang.Count(x => x.TrangThai == "Hoàn thành"),
                    DaHuy = model.DanhSachDonHang.Count(x => x.TrangThai == "Đã hủy")
                };

                // Thống kê theo ngày
                model.ThongKeTheoNgay = Enumerable.Range(0, (model.DenNgay - model.TuNgay).Days + 1)
                    .Select(offset => model.TuNgay.AddDays(offset))
                    .ToDictionary(
                        date => date.ToString("dd/MM"),
                        date => new ThongKeNgayViewModel
                        {
                            Ngay = date,
                            HoanThanh = model.DanhSachDonHang.Count(x =>
                                x.NgayDat.Date == date && x.TrangThai == "Hoàn thành"),
                            DangXuLy = model.DanhSachDonHang.Count(x =>
                                x.NgayDat.Date == date &&
                                (x.TrangThai == "Đang xử lý" || x.TrangThai == "Đang giao")),
                            Huy = model.DanhSachDonHang.Count(x =>
                                x.NgayDat.Date == date && x.TrangThai == "Đã hủy")
                        }
                    );

                // Dữ liệu cho biểu đồ
                var labels = model.ThongKeTheoNgay.Keys.ToArray();
                ViewBag.Labels = labels;
                ViewBag.HoanThanhData = labels.Select(l => model.ThongKeTheoNgay[l].HoanThanh).ToArray();
                ViewBag.DangXuLyData = labels.Select(l => model.ThongKeTheoNgay[l].DangXuLy).ToArray();
                ViewBag.HuyData = labels.Select(l => model.ThongKeTheoNgay[l].Huy).ToArray();

                return View(model);
            }
            catch (Exception ex)
            {
                // Log lỗi
                return View(new TheoDoiDonHangViewModel());
            }
        }
        #endregion
    }
}