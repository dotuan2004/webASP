use WebThoiTrang
GO

-- View tổng hợp doanh thu theo sản phẩm
CREATE VIEW v_DoanhThuSanPham AS
SELECT 
    sp.MaSP,
    sp.TenSP,
    COUNT(DISTINCT nk.MaDH) as SoDonHang,
    SUM(nk.SoLuong) as TongSoLuongBan,
    SUM(nk.ThanhTien) as TongDoanhThu
FROM SanPham sp
LEFT JOIN NhatKyBanHang nk ON sp.MaSP = nk.MaSP
GROUP BY sp.MaSP, sp.TenSP
GO

-- View tổng hợp công nợ khách hàng
CREATE VIEW v_CongNoKhachHang AS
SELECT 
    kh.MaKH,
    kh.TenKhachHang,
    SUM(cn.SoTienNo) as TongNo,
    SUM(cn.DaThanhToan) as TongDaThanhToan,
    SUM(cn.ConLai) as TongConLai
FROM KhachHang kh
LEFT JOIN CongNo cn ON kh.MaKH = cn.MaKH
GROUP BY kh.MaKH, kh.TenKhachHang
GO

-- View chi tiết sản phẩm với khuyến mãi
CREATE VIEW v_ChiTietSanPham AS
SELECT 
    sp.MaSP,
    sp.TenSP,
    sp.GiaSP,
    sp.SoLuongTon,
    l.TenLoai,
    ncc.TenNCC,
    km.TenKM,
    km.PhanTramGiam,
    CASE 
        WHEN km.MaKM IS NOT NULL AND GETDATE() BETWEEN km.NgayBatDau AND km.NgayKetThuc
        THEN sp.GiaSP * (1 - km.PhanTramGiam/100)
        ELSE sp.GiaSP
    END as GiaSauKhuyenMai
FROM SanPham sp
JOIN Loai l ON sp.MaLoai = l.MaLoai
JOIN NCC ncc ON sp.MaNCC = ncc.MaNCC
LEFT JOIN SanPham_KhuyenMai spkm ON sp.MaSP = spkm.MaSP
LEFT JOIN KhuyenMai km ON spkm.MaKM = km.MaKM
    AND GETDATE() BETWEEN km.NgayBatDau AND km.NgayKetThuc;
GO

-- View thống kê theo danh mục
CREATE VIEW v_ThongKeTheoDanhMuc AS
SELECT 
    l.MaLoai,
    l.TenLoai,
    COUNT(sp.MaSP) as SoSanPham,
    SUM(sp.SoLuongTon) as TongTonKho,
    SUM(sp.SoLuongTon * sp.GiaVon) as GiaTriTonKho,
    (
        SELECT COUNT(DISTINCT ctdh.MaDH)
        FROM ChiTietDonHang ctdh
        JOIN SanPham sp2 ON ctdh.MaSP = sp2.MaSP
        WHERE sp2.MaLoai = l.MaLoai
    ) as SoDonHang,
    (
        SELECT SUM(ctdh.TongTien)
        FROM ChiTietDonHang ctdh
        JOIN SanPham sp2 ON ctdh.MaSP = sp2.MaSP
        WHERE sp2.MaLoai = l.MaLoai
    ) as DoanhThu
FROM Loai l
LEFT JOIN SanPham sp ON l.MaLoai = sp.MaLoai
GROUP BY l.MaLoai, l.TenLoai;
GO

-- View báo cáo tổng hợp
CREATE VIEW v_BaoCaoTongHop AS
SELECT 
    CONVERT(DATE, dh.NgayDatHang) as Ngay,
    COUNT(DISTINCT dh.MaDH) as SoDonHang,
    SUM(dh.TongTienDonHang) as DoanhThu,
    SUM(dh.TienThueVAT) as TongThueVAT,
    SUM(
        SELECT SUM(ctdh.SoLuongMua * sp.GiaVon)
        FROM ChiTietDonHang ctdh
        JOIN SanPham sp ON ctdh.MaSP = sp.MaSP
        WHERE ctdh.MaDH = dh.MaDH
    ) as GiaVon,
    SUM(dh.TongTienDonHang) - SUM(
        SELECT SUM(ctdh.SoLuongMua * sp.GiaVon)
        FROM ChiTietDonHang ctdh
        JOIN SanPham sp ON ctdh.MaSP = sp.MaSP
        WHERE ctdh.MaDH = dh.MaDH
    ) as LoiNhuan
FROM DonHang dh
GROUP BY CONVERT(DATE, dh.NgayDatHang);
GO