using System.Collections.Generic;
using System;

namespace LTW.Areas.Admin.Model
{
    public class TopSanPhamViewModel
    {
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public int Top { get; set; }
        public List<TopSanPhamDetail> DanhSachSanPham { get; set; }
        public decimal TongDoanhThu { get; set; }
        public int TongSoLuong { get; set; }
    }
}