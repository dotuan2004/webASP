using System.Collections.Generic;

namespace LTW.Areas.Admin.Model
{
    public class SanPhamHetHangViewModel
    {
        public int SoLuongHetHang { get; set; }
        public int SoLuongSapHet { get; set; }
        public int SoLuongDaDatHang { get; set; }
        public decimal GiaTriCanNhap { get; set; }
        public List<SanPhamHetHangDetail> DanhSachSanPham { get; set; }
        public Dictionary<string, int> ThongKeTheoLoai { get; set; }
    }
}