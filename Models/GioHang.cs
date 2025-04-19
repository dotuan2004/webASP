using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace LTW.Models
{
    public partial class CartItem
    {
        MyDataDataContext data = new MyDataDataContext();

        [Display(Name = "Mã Sản Phẩm")]
        public int MaSP { get; set; }

        [Display(Name = "Tên Sản Phẩm")]
        public string TenSanPham { get; set; }

        [Display(Name = "Ảnh bìa")]
        public string hinh { get; set; }

        [Display(Name = "Giá bán")]
        public Double giaban { get; set; }

        [Display(Name = "Số lượng")]
        public int isoluong { get; set; }

        [Display(Name = "Thành tiền")]
        public Double dThanhtien
        {
            get { return isoluong * giaban; }
        }

        // Constructor không tham số
        public CartItem() { }

        // Constructor cho Session (giữ nguyên của bạn)
        public CartItem(int id)
        {
            MaSP = id;
            var sp = data.SanPhams.Single(n => n.MaSP == MaSP);
            TenSanPham = sp.TenSP;
            hinh = sp.Hinh;
            giaban = double.Parse(sp.GiaSP.ToString());
            isoluong = 1;
        }

        // Constructor mới cho Database
        public CartItem(ChiTietGioHang ctgh)
        {
            var sp = data.SanPhams.Single(n => n.MaSP == ctgh.MaSP);
            MaSP = ctgh.MaSP;
            TenSanPham = sp.TenSP;
            hinh = sp.Hinh;
            giaban = double.Parse(ctgh.DonGia.ToString());
            isoluong = ctgh.SoLuong;
        }

        // Phương thức chuyển đổi sang ChiTietGioHang
        public ChiTietGioHang ToChiTietGioHang(int maGioHang)
        {
            return new ChiTietGioHang
            {
                MaGioHang = maGioHang,
                MaSP = this.MaSP,
                SoLuong = this.isoluong,
                DonGia = (decimal)this.giaban,
                ThanhTien = (decimal)this.dThanhtien,
                NgayThem = DateTime.Now
            };
        }

        // Kiểm tra số lượng tồn
        public bool KiemTraSoLuong(int themSoLuong = 0)
        {
            var sp = data.SanPhams.SingleOrDefault(n => n.MaSP == MaSP);
            return sp != null && sp.SoLuongTon >= (isoluong + themSoLuong);
        }
    }
}