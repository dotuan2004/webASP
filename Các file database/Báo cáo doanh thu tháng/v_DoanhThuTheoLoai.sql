use WebThoiTrang
GO

CREATE VIEW v_DoanhThuTheoLoai AS
SELECT 
    l.MaLoai,
    l.TenLoai,
    COUNT(DISTINCT dh.MaDH) as SoDonHang,
    SUM(ctdh.SoLuongMua) as SoLuongBan,
    SUM(ctdh.TongTien) as DoanhThu,
    SUM(ctdh.SoLuongMua * sp.GiaVon) as TongChiPhi
FROM DonHang dh
JOIN ChiTietDonHang ctdh ON dh.MaDH = ctdh.MaDH
JOIN SanPham sp ON ctdh.MaSP = sp.MaSP
JOIN Loai l ON sp.MaLoai = l.MaLoai
WHERE dh.TrangThai = 'dagiao'
GROUP BY l.MaLoai, l.TenLoai;