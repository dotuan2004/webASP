use WebThoiTrang
Go

CREATE PROCEDURE sp_BaoCaoDoanhThuThang
    @Thang INT,
    @Nam INT
AS
BEGIN
    -- Thống kê tổng quan tháng hiện tại
    SELECT 
        COUNT(DISTINCT dh.MaDH) as SoDonHang,
        SUM(dh.TongTienDonHang) as TongDoanhThu,
        COUNT(DISTINCT dh.MaKH) as SoKhachHang,
        SUM(ct.SoLuongMua * sp.GiaVon) as TongChiPhi,
        SUM(dh.TongTienDonHang) - SUM(ct.SoLuongMua * sp.GiaVon) as LoiNhuan
    FROM DonHang dh
    JOIN ChiTietDonHang ct ON dh.MaDH = ct.MaDH
    JOIN SanPham sp ON ct.MaSP = sp.MaSP
    WHERE MONTH(dh.NgayDatHang) = @Thang 
        AND YEAR(dh.NgayDatHang) = @Nam
        AND dh.TrangThai = 'dagiao';

    -- Thống kê theo ngày trong tháng
    SELECT 
        DAY(dh.NgayDatHang) as Ngay,
        COUNT(DISTINCT dh.MaDH) as SoDonHang,
        SUM(dh.TongTienDonHang) as DoanhThu
    FROM DonHang dh
    WHERE MONTH(dh.NgayDatHang) = @Thang 
        AND YEAR(dh.NgayDatHang) = @Nam
        AND dh.TrangThai = 'dagiao'
    GROUP BY DAY(dh.NgayDatHang)
    ORDER BY Ngay;
END