use WebThoiTrang
GO

-- Stored Procedure báo cáo sản phẩm sắp hết
CREATE PROCEDURE sp_BaoCaoSanPhamSapHet
    @MucTonToiThieu INT = 10
AS
BEGIN
    -- Thống kê tổng quan
    SELECT 
        COUNT(CASE WHEN SoLuongTon = 0 THEN 1 END) as HetHang,
        COUNT(CASE WHEN SoLuongTon BETWEEN 1 AND @MucTonToiThieu THEN 1 END) as SapHet,
        COUNT(CASE WHEN DangDatHang > 0 THEN 1 END) as DaDatHang,
        SUM(CASE 
            WHEN DaBan30Ngay > 0 
            THEN CEILING(DaBan30Ngay * 1.5) - SoLuongTon
            ELSE @MucTonToiThieu - SoLuongTon
        END * GiaVon) as GiaTriCanNhap
    FROM v_TonKho
    WHERE SoLuongTon <= @MucTonToiThieu;

    -- Chi tiết sản phẩm sắp hết
    SELECT 
        MaSP,
        TenSP,
        LoaiSP,
        NhaCungCap,
        SoLuongTon,
        DaBan30Ngay,
        GiaVon,
        CASE 
            WHEN DaBan30Ngay > 0 
            THEN CEILING(DaBan30Ngay * 1.5) - SoLuongTon
            ELSE @MucTonToiThieu - SoLuongTon
        END as SoLuongCanNhap,
        CASE 
            WHEN SoLuongTon = 0 THEN N'Hết hàng'
            ELSE N'Sắp hết'
        END as TrangThai,
        DangDatHang
    FROM v_TonKho
    WHERE SoLuongTon <= @MucTonToiThieu
    ORDER BY SoLuongTon;

    -- Thống kê theo loại sản phẩm
    SELECT 
        LoaiSP,
        COUNT(*) as SoLuong
    FROM v_TonKho
    WHERE SoLuongTon <= @MucTonToiThieu
    GROUP BY LoaiSP
    ORDER BY SoLuong DESC;
END