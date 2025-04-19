namespace LTW.Areas.Admin.Model
{
    public class SanPhamHetHangDetail
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public string LoaiSP { get; set; }
        public string NhaCungCap { get; set; }
        public int SoLuongTon { get; set; }
        public int DaBan30Ngay { get; set; }
        public decimal GiaVon { get; set; }
        public int SoLuongCanNhap { get; set; }
        public string TrangThai => SoLuongTon == 0 ? "Hết hàng" : "Sắp hết";
        public string TrangThaiClass => SoLuongTon == 0 ? "bg-danger text-white" : "bg-warning text-dark";
    }
}