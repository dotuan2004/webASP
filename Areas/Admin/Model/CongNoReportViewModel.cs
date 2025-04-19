using System;
using System.Collections.Generic;

namespace LTW.Areas.Admin.Model
{
    public class CongNoReportViewModel
    {
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public decimal TongCongNo { get; set; }
        public int SoKhachHangNo { get; set; }
        public decimal NoQuaHan { get; set; }
        public decimal DaThu { get; set; }
        public List<CongNoKhachHangViewModel> DanhSachCongNo { get; set; }
        public decimal[] TuoiNoData { get; set; }
        public Dictionary<string, decimal> CongNoTheoThang { get; set; }
    }
}