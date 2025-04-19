using System.Collections.Generic;
using System;

namespace LTW.Areas.Admin.Model
{
    public class NhatKyBanHangViewModel
    {
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public string TrangThai { get; set; }
        public ThongKeNhatKyViewModel ThongKe { get; set; }
        public List<NhatKyBanHangDetailViewModel> DanhSachNhatKy { get; set; }
        public Dictionary<string, decimal> DoanhThuNgay { get; set; }
    }
}