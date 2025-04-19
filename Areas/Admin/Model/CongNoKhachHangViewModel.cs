using System;
using System.Collections.Generic;

namespace LTW.Areas.Admin.Model
{
    public class CongNoKhachHangViewModel
    {
        public int MaKH { get; set; }
        public string TenKH { get; set; }
        public string SoDienThoai { get; set; }
        public decimal TongNo { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConLai { get; set; }
        public DateTime HanThanhToan { get; set; }
        public int NgayQuaHan { get; set; }
        public string TrangThai
        {
            get
            {
                if (ConLai == 0) return "Đã thanh toán";
                if (NgayQuaHan > 0) return "Quá hạn";
                return "Trong hạn";
            }
        }
        public string TrangThaiClass
        {
            get
            {
                if (ConLai == 0) return "bg-success text-white";
                if (NgayQuaHan > 0) return "bg-danger text-white";
                return "bg-warning text-dark";
            }
        }
    }
}