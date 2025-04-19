using System;

namespace LTW.Areas.Admin.Model
{
    public class DoanhThuThangViewModel
    {
        public DateTime NgayBaoCao { get; set; }
        public int SoDonHang { get; set; }
        public decimal DoanhThu { get; set; }
        public int SoKhachHang { get; set; }
        public decimal TongChiPhi { get; set; }
        public decimal LoiNhuan { get; set; }
        public decimal TrungBinhDonHang { get; set; }
        public decimal[] ChartData { get; set; }
        public decimal[] DistributionData { get; set; }
        public decimal PhanTramDoanhThu { get; set; }
        public decimal PhanTramDonHang { get; set; }
        public decimal PhanTramKhachHang { get; set; }
    }
}