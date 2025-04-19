use WebThoiTrang
GO

CREATE VIEW v_ThongKeSanPham AS
SELECT 
    sp.MaSP,
    sp.TenSP,
    sp.Hinh,
    l.TenLoai as LoaiSP,
    ncc.TenNCC as NhaCungCap,
    COUNT(DISTINCT ctdh.MaDH) as SoDonHang,
    SUM(ctdh.SoLuongMua) as SoLuongBan,
    SUM(ctdh.TongTien) as DoanhThu,
    sp.GiaVon,
    SUM(ctdh.TongTien - (ctdh.SoLuongMua * sp.GiaVon)) as LoiNhuan,
    MAX(dh.NgayDatHang) as BanGanNhat
FROM SanPham sp
LEFT JOIN ChiTietDonHang ctdh ON sp.MaSP = ctdh.MaSP
LEFT JOIN DonHang dh ON ctdh.MaDH = dh.MaDH AND dh.TrangThai = 'dagiao'
JOIN Loai l ON sp.MaLoai = l.MaLoai
JOIN NCC ncc ON sp.MaNCC = ncc.MaNCC
GROUP BY sp.MaSP, sp.TenSP, sp.Hinh, l.TenLoai, ncc.TenNCC, sp.GiaVon;