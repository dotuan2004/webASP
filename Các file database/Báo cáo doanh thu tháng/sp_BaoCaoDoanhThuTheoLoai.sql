use WebThoiTrang
GO

CREATE PROCEDURE sp_BaoCaoDoanhThuTheoLoai
    @TuNgay DATE,
    @DenNgay DATE
AS
BEGIN
    SELECT 
        l.MaLoai,
        l.TenLoai as LoaiSP,
        COUNT(DISTINCT dh.MaDH) as SoDonHang,
        SUM(ctdh.SoLuongMua) as SoLuongBan,
        SUM(ctdh.TongTien) as DoanhThu,
        SUM(ctdh.SoLuongMua * sp.GiaVon) as TongChiPhi
    FROM DonHang dh
    JOIN ChiTietDonHang ctdh ON dh.MaDH = ctdh.MaDH
    JOIN SanPham sp ON ctdh.MaSP = sp.MaSP
    JOIN Loai l ON sp.MaLoai = l.MaLoai
    WHERE dh.TrangThai = 'dagiao'
        AND dh.NgayDatHang BETWEEN @TuNgay AND @DenNgay
    GROUP BY l.MaLoai, l.TenLoai
    ORDER BY DoanhThu DESC;
END