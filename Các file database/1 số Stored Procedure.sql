use WebThoiTrang
GO

-- SP lấy báo cáo doanh thu theo khoảng thời gian
CREATE PROCEDURE sp_BaoCaoDoanhThu
    @TuNgay DATE,
    @DenNgay DATE
AS
BEGIN
    SELECT 
        NgayBaoCao,
        SoDonHang,
        TongDoanhThu,
        TongChiPhi,
        LoiNhuan
    FROM DoanhThu
    WHERE NgayBaoCao BETWEEN @TuNgay AND @DenNgay
    ORDER BY NgayBaoCao DESC
END
GO

-- SP lấy báo cáo tồn kho theo sản phẩm
CREATE PROCEDURE sp_BaoCaoTonKhoSanPham
    @MaSP INT = NULL
AS
BEGIN
    SELECT 
        sp.MaSP,
        sp.TenSP,
        sp.SoLuongTon,
        ISNULL(bctk.NhapTrongKy, 0) as NhapTrongKy,
        ISNULL(bctk.XuatTrongKy, 0) as XuatTrongKy
    FROM SanPham sp
    LEFT JOIN BaoCaoTonKho bctk ON sp.MaSP = bctk.MaSP
    WHERE (@MaSP IS NULL OR sp.MaSP = @MaSP)
END
GO

-- SP lấy công nợ của khách hàng
CREATE PROCEDURE sp_BaoCaoCongNo
    @MaKH INT = NULL,
    @TrangThai NVARCHAR(50) = NULL
AS
BEGIN
    SELECT 
        cn.MaCN,
        kh.TenKhachHang,
        cn.NgayPhatSinh,
        cn.SoTienNo,
        cn.DaThanhToan,
        cn.ConLai,
        cn.TrangThai
    FROM CongNo cn
    JOIN KhachHang kh ON cn.MaKH = kh.MaKH
    WHERE (@MaKH IS NULL OR cn.MaKH = @MaKH)
    AND (@TrangThai IS NULL OR cn.TrangThai = @TrangThai)
END
GO

-- SP xử lý đơn hàng
CREATE PROCEDURE sp_XuLyDonHang
    @MaDH INT,
    @TrangThai VARCHAR(20),
    @GhiChu NVARCHAR(MAX) = NULL,
    @NguoiXuLy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Kiểm tra đơn hàng tồn tại
    IF NOT EXISTS (SELECT 1 FROM DonHang WHERE MaDH = @MaDH)
    BEGIN
        RAISERROR (N'Đơn hàng không tồn tại!', 16, 1);
        RETURN;
    END

    -- Cập nhật trạng thái đơn hàng
    UPDATE DonHang 
    SET 
        TrangThai = @TrangThai,
        GhiChu = ISNULL(@GhiChu, GhiChu)
    WHERE MaDH = @MaDH;

    -- Ghi nhật ký xử lý
    INSERT INTO NhatKyBanHang (MaDH, MaTT, MaSP, SoLuong, DonGia, ThanhTien, TrangThai)
    SELECT 
        dh.MaDH,
        dh.MaTT,
        ctdh.MaSP,
        ctdh.SoLuongMua,
        sp.GiaSP,
        ctdh.TongTien,
        @TrangThai
    FROM DonHang dh
    JOIN ChiTietDonHang ctdh ON dh.MaDH = ctdh.MaDH
    JOIN SanPham sp ON ctdh.MaSP = sp.MaSP
    WHERE dh.MaDH = @MaDH;
END;
GO

-- SP Thống kê báo cáo chi tiết
CREATE PROCEDURE sp_ThongKeChiTiet
    @TuNgay DATE,
    @DenNgay DATE,
    @LoaiBaoCao NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @LoaiBaoCao = N'DoanhThu'
    BEGIN
        SELECT 
            sp.MaLoai,
            l.TenLoai,
            COUNT(DISTINCT dh.MaDH) as SoDonHang,
            SUM(ctdh.SoLuongMua) as SoLuongBan,
            SUM(ctdh.TongTien) as DoanhThu,
            SUM(ctdh.SoLuongMua * sp.GiaVon) as GiaVon,
            SUM(ctdh.TongTien - (ctdh.SoLuongMua * sp.GiaVon)) as LoiNhuan
        FROM DonHang dh
        JOIN ChiTietDonHang ctdh ON dh.MaDH = ctdh.MaDH
        JOIN SanPham sp ON ctdh.MaSP = sp.MaSP
        JOIN Loai l ON sp.MaLoai = l.MaLoai
        WHERE dh.NgayDatHang BETWEEN @TuNgay AND @DenNgay
        GROUP BY sp.MaLoai, l.TenLoai
        ORDER BY DoanhThu DESC;
    END
    ELSE IF @LoaiBaoCao = N'SanPhamBanChay'
    BEGIN
        SELECT TOP 10
            sp.MaSP,
            sp.TenSP,
            SUM(ctdh.SoLuongMua) as SoLuongBan,
            SUM(ctdh.TongTien) as DoanhThu
        FROM DonHang dh
        JOIN ChiTietDonHang ctdh ON dh.MaDH = ctdh.MaDH
        JOIN SanPham sp ON ctdh.MaSP = sp.MaSP
        WHERE dh.NgayDatHang BETWEEN @TuNgay AND @DenNgay
        GROUP BY sp.MaSP, sp.TenSP
        ORDER BY SoLuongBan DESC;
    END
END;
GO

-- Nhật ký bán hàng chi tiết
SELECT 
    nk.NgayGiaoDich,
    dh.MaDH,
    kh.TenKhachHang,
    sp.TenSP,
    l.TenLoai,
    nk.SoLuong,
    nk.DonGia,
    nk.ThanhTien,
    tt.TenTT as HinhThucThanhToan,
    nk.TrangThai
FROM NhatKyBanHang nk
JOIN DonHang dh ON nk.MaDH = dh.MaDH
JOIN KhachHang kh ON dh.MaKH = kh.MaKH
JOIN SanPham sp ON nk.MaSP = sp.MaSP
JOIN Loai l ON sp.MaLoai = l.MaLoai
JOIN ThanhToan tt ON nk.MaTT = tt.MaTT
WHERE nk.NgayGiaoDich BETWEEN :ngayBatDau AND :ngayKetThuc
ORDER BY nk.NgayGiaoDich DESC;

-- Thống kê theo ngày
SELECT 
    DATE(nk.NgayGiaoDich) as Ngay,
    COUNT(DISTINCT dh.MaDH) as SoDonHang,
    COUNT(DISTINCT dh.MaKH) as SoKhachHang,
    SUM(nk.SoLuong) as TongSoLuong,
    SUM(nk.ThanhTien) as DoanhThu,
    STRING_AGG(DISTINCT tt.TenTT, ', ') as HinhThucThanhToan
FROM NhatKyBanHang nk
JOIN DonHang dh ON nk.MaDH = dh.MaDH
JOIN ThanhToan tt ON nk.MaTT = tt.MaTT
WHERE nk.NgayGiaoDich BETWEEN :ngayBatDau AND :ngayKetThuc
GROUP BY DATE(nk.NgayGiaoDich)
ORDER BY Ngay DESC;

-- Báo cáo doanh thu tổng hợp
WITH DoanhThuNgay AS (
    SELECT 
        DATE_TRUNC('day', dh.NgayDatHang) as Ngay,
        COUNT(DISTINCT dh.MaDH) as SoDonHang,
        COUNT(DISTINCT dh.MaKH) as SoKhachHang,
        SUM(dh.TongTienDonHang) as DoanhThu,
        SUM(ctdh.SoLuongMua * sp.GiaVon) as GiaVon,
        SUM(dh.TongTienDonHang) - SUM(ctdh.SoLuongMua * sp.GiaVon) as LoiNhuan
    FROM DonHang dh
    JOIN ChiTietDonHang ctdh ON dh.MaDH = ctdh.MaDH
    JOIN SanPham sp ON ctdh.MaSP = sp.MaSP
    WHERE dh.TrangThai = 'Hoàn thành'
    GROUP BY DATE_TRUNC('day', dh.NgayDatHang)
)
SELECT 
    Ngay,
    SoDonHang,
    SoKhachHang,
    DoanhThu,
    GiaVon,
    LoiNhuan,
    ROUND(LoiNhuan * 100.0 / NULLIF(DoanhThu, 0), 2) as TyLeLoiNhuan,
    ROUND((DoanhThu - LAG(DoanhThu) OVER (ORDER BY Ngay)) * 100.0 
        / NULLIF(LAG(DoanhThu) OVER (ORDER BY Ngay), 0), 2) as TangTruongDoanhThu
FROM DoanhThuNgay
ORDER BY Ngay DESC;

-- Phân tích theo kênh bán hàng
SELECT 
    COALESCE(dh.NguonDon, 'Trực tiếp') as KenhBan,
    COUNT(DISTINCT dh.MaDH) as SoDonHang,
    SUM(dh.TongTienDonHang) as DoanhThu,
    ROUND(AVG(dh.TongTienDonHang), 0) as GiaTriTrungBinh
FROM DonHang dh 
WHERE dh.TrangThai = 'Hoàn thành'
    AND dh.NgayDatHang BETWEEN :ngayBatDau AND :ngayKetThuc
GROUP BY dh.NguonDon
ORDER BY DoanhThu DESC;

-- Báo cáo công nợ chi tiết
SELECT 
    kh.MaKH,
    kh.TenKhachHang,
    kh.SDT,
    cn.NgayPhatSinh,
    cn.SoTienNo,
    cn.DaThanhToan,
    cn.ConLai,
    cn.HanThanhToan,
    CURRENT_DATE - cn.HanThanhToan as SoNgayQuaHan,
    CASE 
        WHEN cn.HanThanhToan < CURRENT_DATE AND cn.ConLai > 0 THEN 'Quá hạn'
        WHEN cn.ConLai = 0 THEN 'Đã thanh toán'
        ELSE 'Trong hạn'
    END as TrangThaiNo
FROM CongNo cn
JOIN KhachHang kh ON cn.MaKH = kh.MaKH
WHERE cn.NgayPhatSinh BETWEEN :ngayBatDau AND :ngayKetThuc;

-- Thống kê tuổi nợ
SELECT 
    CASE 
        WHEN CURRENT_DATE - cn.HanThanhToan <= 0 THEN 'Trong hạn'
        WHEN CURRENT_DATE - cn.HanThanhToan <= 30 THEN '1-30 ngày'
        WHEN CURRENT_DATE - cn.HanThanhToan <= 60 THEN '31-60 ngày'
        WHEN CURRENT_DATE - cn.HanThanhToan <= 90 THEN '61-90 ngày'
        ELSE 'Trên 90 ngày'
    END as TuoiNo,
    COUNT(DISTINCT cn.MaKH) as SoKhachHang,
    SUM(cn.ConLai) as TongNo
FROM CongNo cn
WHERE cn.ConLai > 0
GROUP BY 
    CASE 
        WHEN CURRENT_DATE - cn.HanThanhToan <= 0 THEN 'Trong hạn'
        WHEN CURRENT_DATE - cn.HanThanhToan <= 30 THEN '1-30 ngày'
        WHEN CURRENT_DATE - cn.HanThanhToan <= 60 THEN '31-60 ngày'
        WHEN CURRENT_DATE - cn.HanThanhToan <= 90 THEN '61-90 ngày'
        ELSE 'Trên 90 ngày'
    END
ORDER BY 
    CASE TuoiNo
        WHEN 'Trong hạn' THEN 1
        WHEN '1-30 ngày' THEN 2
        WHEN '31-60 ngày' THEN 3
        WHEN '61-90 ngày' THEN 4
        ELSE 5
    END;

	-- Báo cáo tồn kho chi tiết
SELECT 
    sp.MaSP,
    sp.TenSP,
    l.TenLoai,
    sp.SoLuongTon,
    sp.GiaSP,
    sp.GiaVon,
    sp.SoLuongTon * sp.GiaVon as GiaTriTon,
    (
        SELECT SUM(ctdh.SoLuongMua)
        FROM ChiTietDonHang ctdh
        JOIN DonHang dh ON ctdh.MaDH = dh.MaDH
        WHERE ctdh.MaSP = sp.MaSP
        AND dh.NgayDatHang >= CURRENT_DATE - INTERVAL '30 days'
		) as SoLuongBan30Ngay,
    COALESCE(
        (
            SELECT MAX(dh.NgayDatHang)
            FROM ChiTietDonHang ctdh
            JOIN DonHang dh ON ctdh.MaDH = dh.MaDH
            WHERE ctdh.MaSP = sp.MaSP
        ),
        NULL
    ) as NgayBanGanNhat,
    CASE 
        WHEN sp.SoLuongTon = 0 THEN 'Hết hàng'
        WHEN sp.SoLuongTon <= 10 THEN 'Sắp hết hàng'
        ELSE 'Bình thường'
    END as TrangThaiTon
FROM SanPham sp
JOIN Loai l ON sp.MaLoai = l.MaLoai;

-- Phân tích biến động tồn kho
SELECT 
    bctk.MaSP,
    sp.TenSP,
    l.TenLoai,
    bctk.TonDau,
    bctk.NhapTrongKy,
    bctk.XuatTrongKy,
    bctk.TonCuoi,
    bctk.GiaTriTonKho,
    ROUND(
        (bctk.TonCuoi - bctk.TonDau) * 100.0 / NULLIF(bctk.TonDau, 0), 
        2
    ) as PhanTramBienDong,
    ROUND(
        bctk.XuatTrongKy * 1.0 / NULLIF(
            (bctk.TonDau + bctk.NhapTrongKy), 0
        ) * 100, 
        2
    ) as TyLeXuatKho
FROM BaoCaoTonKho bctk
JOIN SanPham sp ON bctk.MaSP = sp.MaSP
JOIN Loai l ON sp.MaLoai = l.MaLoai
WHERE bctk.NgayBaoCao BETWEEN :ngayBatDau AND :ngayKetThuc;

-- Phân tích vòng quay hàng tồn kho
WITH VongQuayTon AS (
    SELECT 
        sp.MaSP,
        sp.TenSP,
        l.TenLoai,
        sp.SoLuongTon,
        SUM(ctdh.SoLuongMua) as SoLuongBan,
        AVG(sp.SoLuongTon) as TonKhoBinhQuan,
        SUM(ctdh.SoLuongMua * sp.GiaVon) as GiaVonBanHang
    FROM SanPham sp
    JOIN Loai l ON sp.MaLoai = l.MaLoai
    LEFT JOIN ChiTietDonHang ctdh ON sp.MaSP = ctdh.MaSP
    LEFT JOIN DonHang dh ON ctdh.MaDH = dh.MaDH
    WHERE dh.NgayDatHang BETWEEN :ngayBatDau AND :ngayKetThuc
    GROUP BY sp.MaSP, sp.TenSP, l.TenLoai, sp.SoLuongTon
)
SELECT 
    *,
    CASE 
        WHEN TonKhoBinhQuan > 0 THEN 
            ROUND(SoLuongBan * 30.0 / TonKhoBinhQuan, 2)
        ELSE 0 
    END as VongQuayTonKho,
    CASE 
        WHEN SoLuongBan > 0 THEN 
            ROUND(TonKhoBinhQuan * 30.0 / SoLuongBan, 2)
        ELSE 0 
    END as SoNgayTonKho
FROM VongQuayTon
ORDER BY VongQuayTonKho DESC;