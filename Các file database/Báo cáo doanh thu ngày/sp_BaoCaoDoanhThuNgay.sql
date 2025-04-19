use WebThoiTrang
GO

CREATE PROCEDURE sp_BaoCaoDoanhThuNgay
    @NgayBaoCao DATE
AS
BEGIN
    -- Thống kê tổng quan
    SELECT 
        COUNT(DISTINCT dh.MaDH) as SoDonHang,
        SUM(dh.TongTienDonHang) as DoanhThu,
        COUNT(DISTINCT dh.MaKH) as SoKhachHang,
        CASE 
            WHEN COUNT(DISTINCT dh.MaDH) > 0 
            THEN SUM(dh.TongTienDonHang) / COUNT(DISTINCT dh.MaDH)
            ELSE 0 
        END as TrungBinhDonHang
    FROM DonHang dh
    WHERE CONVERT(DATE, dh.NgayDatHang) = @NgayBaoCao
        AND dh.TrangThai = 'dagiao';

    -- Thống kê theo giờ
    SELECT 
        DATEPART(HOUR, dh.NgayDatHang) as Gio,
        SUM(dh.TongTienDonHang) as DoanhThu
    FROM DonHang dh
    WHERE CONVERT(DATE, dh.NgayDatHang) = @NgayBaoCao
        AND dh.TrangThai = 'dagiao'
    GROUP BY DATEPART(HOUR, dh.NgayDatHang)
    ORDER BY Gio;
END