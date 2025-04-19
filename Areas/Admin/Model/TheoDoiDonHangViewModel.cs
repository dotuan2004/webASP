using System;
using System.Collections.Generic;

namespace LTW.Areas.Admin.Model
{
    public class TheoDoiDonHangViewModel
    {
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public string TrangThai { get; set; }
        public ThongKeDonHangViewModel ThongKe { get; set; }
        public List<DonHangViewModel> DanhSachDonHang { get; set; }
        public Dictionary<string, ThongKeNgayViewModel> ThongKeTheoNgay { get; set; }
    }
}