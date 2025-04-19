namespace LTW.Areas.Admin.Model
{
    public class DoanhThuNgayViewModel
    {
        public int SoDonHang { get; set; }
        public decimal DoanhThu { get; set; }
        public decimal TongChiPhi { get; set; }
        public decimal LoiNhuan { get; set; }
        public int SoKhachHang { get; set; }
        public decimal TrungBinhDonHang { get; set; }
        public decimal[] ChartData { get; set; }
    }
}