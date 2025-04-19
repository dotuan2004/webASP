use WebThoiTrang
GO

-- View theo dõi tồn kho
CREATE OR ALTER VIEW v_TonKho AS
SELECT 
    sp.MaSP,
    sp.TenSP,
    l.TenLoai as LoaiSP,
    ncc.TenNCC as NhaCungCap,
    sp.SoLuongTon,
    sp.GiaVon,
    -- Tính số lượng đã bán trong 30 ngày gần nhất
    ISNULL((
        SELECT SUM(ctdh.SoLuongMua)
        FROM ChiTietDonHang ctdh
        JOIN DonHang dh ON ctdh.MaDH = dh.MaDH
        WHERE ctdh.MaSP = sp.MaSP
            AND dh.TrangThai = 'dagiao'
            AND dh.NgayDatHang >= DATEADD(DAY, -30, GETDATE())
    ), 0) as DaBan30Ngay,
    -- Kiểm tra số lượng đang đặt trong phiếu nhập
    ISNULL((
        SELECT SUM(ctpn.SoLuong)
        FROM ChiTietPhieuNhap ctpn
        JOIN PhieuNhap pn ON ctpn.MaPN = pn.MaPN
        WHERE ctpn.MaSP = sp.MaSP
            AND (pn.TrangThai = N'Đang đặt' OR pn.TrangThai IS NULL)
    ), 0) as DangDatHang
FROM SanPham sp
JOIN Loai l ON sp.MaLoai = l.MaLoai
JOIN NCC ncc ON sp.MaNCC = ncc.MaNCC;