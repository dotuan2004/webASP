use WebThoiTrang
GO

CREATE PROCEDURE sp_BaoCaoTopSanPham
    @TuNgay DATE,
    @DenNgay DATE,
    @Top INT = 10,
    @TrangThai NVARCHAR(50) = NULL
AS
BEGIN
    -- Thống kê sản phẩm trong kỳ
    WITH SanPhamTrongKy AS (
        SELECT 
            sp.MaSP,
            sp.TenSP,
            sp.Hinh,
            l.TenLoai as LoaiSP,
            COUNT(DISTINCT ctdh.MaDH) as SoDonHang,
            SUM(ctdh.SoLuongMua) as SoLuongBan,
            SUM(ctdh.TongTien) as DoanhThu
        FROM SanPham sp
        LEFT JOIN ChiTietDonHang ctdh ON sp.MaSP = ctdh.MaSP
        LEFT JOIN DonHang dh ON ctdh.MaDH = dh.MaDH
            AND dh.NgayDatHang BETWEEN @TuNgay AND @DenNgay
            AND (@TrangThai IS NULL OR dh.TrangThai = @TrangThai)
        JOIN Loai l ON sp.MaLoai = l.MaLoai
        GROUP BY sp.MaSP, sp.TenSP, sp.Hinh, l.TenLoai
    )
    SELECT TOP (@Top)
        *,
        CASE 
            WHEN SUM(DoanhThu) OVER() > 0 
            THEN (DoanhThu * 100.0 / SUM(DoanhThu) OVER())
            ELSE 0 
        END as TyTrong
    FROM SanPhamTrongKy
    WHERE DoanhThu > 0
    ORDER BY DoanhThu DESC;
END