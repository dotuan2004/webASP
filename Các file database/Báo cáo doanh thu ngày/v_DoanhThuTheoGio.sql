use WebThoiTrang
Go

CREATE VIEW v_DoanhThuTheoGio AS
SELECT 
    DATEPART(HOUR, dh.NgayDatHang) as Gio,
    COUNT(DISTINCT dh.MaDH) as SoDonHang,
    SUM(dh.TongTienDonHang) as DoanhThu,
    COUNT(DISTINCT dh.MaKH) as SoKhachHang
FROM DonHang dh
WHERE dh.TrangThai = 'dagiao'
GROUP BY DATEPART(HOUR, dh.NgayDatHang);