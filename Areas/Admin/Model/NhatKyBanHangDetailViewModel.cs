using System;

namespace LTW.Areas.Admin.Model
{
    public class NhatKyBanHangDetailViewModel
    {
        public DateTime NgayGiaoDich { get; set; }
        public int MaDH { get; set; }
        public string TenSP { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
        public string ThanhToan { get; set; }
        public string TrangThai { get; set; }
        public string TrangThaiClass
        {
            get
            {
                if (TrangThai == "dagiao")
                    return "bg-success";
                else if (TrangThai == "chuagiao")
                    return "bg-warning";
                else if (TrangThai == "dahuy")
                    return "bg-danger";
                else
                    return "bg-secondary";
            }
        }
        public string TrangThaiText
        {
            get
            {
                if (TrangThai == "dagiao")
                    return "Hoàn thành";
                else if (TrangThai == "chuagiao")
                    return "Chưa giao";
                else if (TrangThai == "dahuy")
                    return "Đã hủy";
                else
                    return TrangThai;
            }
        }
    }
}