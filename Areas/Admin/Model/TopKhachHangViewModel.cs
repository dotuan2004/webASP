using System.Collections.Generic;
using System;

namespace LTW.Areas.Admin.Model
{
    public class TopKhachHangViewModel
    {
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public int Top { get; set; }
        public decimal TongDoanhThu { get; set; }
        public int TongSoDonHang { get; set; }
        public List<TopKhachHangDetail> DanhSachKhachHang { get; set; }
    }
}