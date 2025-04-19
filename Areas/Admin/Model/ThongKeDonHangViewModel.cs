using System;

namespace LTW.Areas.Admin.Model
{
    public class ThongKeDonHangViewModel
    {
        public int TongDon { get; set; }
        public int DangXuLy { get; set; }
        public int DangGiao { get; set; }
        public int HoanThanh { get; set; }
        public int DaHuy { get; set; }

        public decimal PhanTramDangXuLy => TongDon > 0 ? (DangXuLy * 100M / TongDon) : 0;
        public decimal PhanTramDangGiao => TongDon > 0 ? (DangGiao * 100M / TongDon) : 0;
        public decimal PhanTramHoanThanh => TongDon > 0 ? (HoanThanh * 100M / TongDon) : 0;
        public decimal PhanTramDaHuy => TongDon > 0 ? (DaHuy * 100M / TongDon) : 0;
    }
}