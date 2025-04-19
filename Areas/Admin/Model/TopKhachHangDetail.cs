using System;

namespace LTW.Areas.Admin.Model
{
    public class TopKhachHangDetail
    {
        public int MaKH { get; set; }
        public string TenKH { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public int SoDonHang { get; set; }
        public decimal TongGiaTri { get; set; }
        public DateTime MuaGanNhat { get; set; }
        public decimal TyLeDonHang { get; set; }
        public decimal TyLeDoanhThu { get; set; }
        public decimal TrungBinhDonHang => SoDonHang > 0 ? TongGiaTri / SoDonHang : 0;
    }
}