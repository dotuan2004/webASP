use WebThoiTrang
GO

CREATE VIEW v_ThongKeKhachHang AS
SELECT 
    kh.MaKH,
    kh.TenKhachHang,
    kh.Email,
    kh.SDT,
    COUNT(dh.MaDH) as SoDonHang,
    SUM(dh.TongTienDonHang) as TongGiaTri,
    MAX(dh.NgayDatHang) as MuaGanNhat
FROM KhachHang kh
LEFT JOIN DonHang dh ON kh.MaKH = dh.MaKH
    AND dh.TrangThai = 'dagiao'
GROUP BY kh.MaKH, kh.TenKhachHang, kh.Email, kh.SDT;