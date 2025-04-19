using System;

namespace LTW.Areas.Admin.Model
{
    public class PhieuNhapViewModel
    {
        public int MaPN { get; set; }
        public DateTime? NgayNhap { get; set; }
        public string TenNCC { get; set; }
        public decimal? TongTien { get; set; }
        public string NguoiNhap { get; set; }
        public string GhiChu { get; set; }
    }
}