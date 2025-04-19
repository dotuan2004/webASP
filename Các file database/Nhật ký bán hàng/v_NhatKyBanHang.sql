use WebThoiTrang
GO

CREATE VIEW v_NhatKyBanHang AS
SELECT 
    nk.MaNK,
    nk.NgayGiaoDich,
    nk.MaDH,
    sp.TenSP,
    nk.SoLuong,
    nk.DonGia,
    nk.ThanhTien,
    dh.TrangThai,
    tt.TenTT as ThanhToan,
    sp.GiaVon,
    sp.GiaSP,
    (nk.ThanhTien - (sp.GiaVon * nk.SoLuong)) as LoiNhuan
FROM NhatKyBanHang nk
JOIN SanPham sp ON nk.MaSP = sp.MaSP
JOIN DonHang dh ON nk.MaDH = dh.MaDH
JOIN ThanhToan tt ON nk.MaTT = tt.MaTT;