namespace LTW.Areas.Admin.Model
{
    public class ThongKeNhatKyViewModel
    {
        public int SoDonHang { get; set; }
        public decimal DoanhThu { get; set; }
        public int SoSanPham { get; set; }
        public decimal TrungBinhDon => SoDonHang > 0 ? DoanhThu / SoDonHang : 0;
    }
}