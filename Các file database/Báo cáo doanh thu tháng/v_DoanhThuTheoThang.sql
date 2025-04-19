use WebThoiTrang
GO

CREATE VIEW v_DoanhThuTheoThang AS
SELECT 
    DATEPART(MONTH, dh.NgayDatHang) as Thang,
    DATEPART(YEAR, dh.NgayDatHang) as Nam,
    COUNT(DISTINCT dh.MaDH) as SoDonHang,
    SUM(dh.TongTienDonHang) as TongDoanhThu,
    COUNT(DISTINCT dh.MaKH) as SoKhachHang,
    SUM(dh.TongTienDonHang) / COUNT(DISTINCT dh.MaDH) as TrungBinhDonHang,
    SUM(ct.SoLuongMua * sp.GiaVon) as TongChiPhi
FROM DonHang dh
JOIN ChiTietDonHang ct ON dh.MaDH = ct.MaDH
JOIN SanPham sp ON ct.MaSP = sp.MaSP
WHERE dh.TrangThai = 'dagiao'
GROUP BY 
    DATEPART(MONTH, dh.NgayDatHang),
    DATEPART(YEAR, dh.NgayDatHang);