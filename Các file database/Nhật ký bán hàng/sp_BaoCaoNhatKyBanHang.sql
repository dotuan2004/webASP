use WebThoiTrang
GO

CREATE PROCEDURE sp_BaoCaoNhatKyBanHang
    @TuNgay DATE,
    @DenNgay DATE,
    @TrangThai NVARCHAR(50) = NULL
AS
BEGIN
    -- Thống kê tổng quan
    SELECT 
        COUNT(DISTINCT MaDH) as SoDonHang,
        SUM(ThanhTien) as DoanhThu,
        SUM(SoLuong) as SoSanPham,
        SUM(LoiNhuan) as LoiNhuan
    FROM v_NhatKyBanHang
    WHERE NgayGiaoDich BETWEEN @TuNgay AND @DenNgay
        AND (@TrangThai IS NULL OR TrangThai = @TrangThai);

    -- Chi tiết nhật ký bán hàng
    SELECT *
    FROM v_NhatKyBanHang
    WHERE NgayGiaoDich BETWEEN @TuNgay AND @DenNgay
        AND (@TrangThai IS NULL OR TrangThai = @TrangThai)
    ORDER BY NgayGiaoDich DESC;

    -- Thống kê theo ngày
    SELECT 
        CAST(NgayGiaoDich AS DATE) as Ngay,
        COUNT(DISTINCT MaDH) as SoDonHang,
        SUM(ThanhTien) as DoanhThu,
        SUM(SoLuong) as SoSanPham
    FROM v_NhatKyBanHang
    WHERE NgayGiaoDich BETWEEN @TuNgay AND @DenNgay
        AND (@TrangThai IS NULL OR TrangThai = @TrangThai)
    GROUP BY CAST(NgayGiaoDich AS DATE)
    ORDER BY Ngay;
END