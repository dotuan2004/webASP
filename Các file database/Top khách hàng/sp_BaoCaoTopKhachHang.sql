use WebThoiTrang
GO

CREATE PROCEDURE sp_BaoCaoTopKhachHang
    @TuNgay DATE,
    @DenNgay DATE,
    @Top INT = 10
AS
BEGIN
    -- Top khách hàng theo doanh thu
    WITH KhachHangTrongKy AS (
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
            AND dh.NgayDatHang BETWEEN @TuNgay AND @DenNgay
        GROUP BY kh.MaKH, kh.TenKhachHang, kh.Email, kh.SDT
    )
    SELECT TOP (@Top) *,
           CASE 
               WHEN SUM(TongGiaTri) OVER() > 0 
               THEN (TongGiaTri * 100.0 / SUM(TongGiaTri) OVER())
               ELSE 0 
           END as TyTrong
    FROM KhachHangTrongKy
    WHERE TongGiaTri > 0
    ORDER BY TongGiaTri DESC;

    -- Thống kê theo ngày
    SELECT 
        CAST(dh.NgayDatHang as DATE) as Ngay,
        COUNT(DISTINCT dh.MaDH) as SoDonHang,
        SUM(dh.TongTienDonHang) as DoanhThu
    FROM DonHang dh
    WHERE dh.TrangThai = 'dagiao'
        AND dh.NgayDatHang BETWEEN @TuNgay AND @DenNgay
    GROUP BY CAST(dh.NgayDatHang as DATE)
    ORDER BY Ngay;
END