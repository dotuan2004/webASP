using System;
using System.ComponentModel.DataAnnotations.Schema;

public class DonHangViewModel
{
    public int MaDH { get; set; }
    public DateTime NgayDat { get; set; }
    public string KhachHang { get; set; }
    public string SoDienThoai { get; set; }
    public decimal TongTien { get; set; }
    public string ThanhToan { get; set; }
    public DateTime? NgayGiao { get; set; }
    public string TrangThai { get; set; }
    [NotMapped]
    public decimal DaThanhToan { get; set; }

    [NotMapped]
    public decimal ConLai
    {
        get { return TongTien - DaThanhToan; }
    }
    public string TrangThaiClass
    {
        get
        {
            switch (TrangThai)
            {
                case "Đang xử lý": return "bg-warning text-dark";
                case "Đang giao": return "bg-info text-white";
                case "Đã giao": return "bg-success text-white";
                case "Đã hủy": return "bg-danger text-white";
                default: return "";
            }
        }
    }
}