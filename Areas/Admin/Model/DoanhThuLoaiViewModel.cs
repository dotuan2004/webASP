using System.Collections.Generic;
using System;

namespace LTW.Areas.Admin.Model
{
    public class DoanhThuLoaiViewModel
    {
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public List<DoanhThuLoaiDetail> DoanhThuLoai { get; set; }
        public decimal TongDoanhThu { get; set; }
        public decimal TongChiPhi { get; set; }
        public decimal LoiNhuan { get; set; }
    }
}