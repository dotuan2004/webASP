namespace LTW.Areas.Admin.Model
{
    public class TopSanPhamDetail
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public string Hinh { get; set; }
        public int SoLuongBan { get; set; }
        public decimal DoanhThu { get; set; }
        public decimal TyTrong { get; set; }
        public decimal GiaTrungBinh => SoLuongBan > 0 ? DoanhThu / SoLuongBan : 0;
    }
}