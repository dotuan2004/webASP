use WebThoiTrang
GO

CREATE TRIGGER trg_CapNhatNhatKyBanHang
ON ChiTietDonHang
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO NhatKyBanHang (
        NgayGiaoDich,
        MaDH,
        MaTT,
        MaSP,
        SoLuong,
        DonGia,
        ThanhTien,
        TrangThai
    )
    SELECT 
        dh.NgayDatHang,
        i.MaDH,
        i.MaTT,
        i.MaSP,
        i.SoLuongMua,
        sp.GiaSP,
        i.TongTien,
        dh.TrangThai
    FROM inserted i
    JOIN DonHang dh ON i.MaDH = dh.MaDH
    JOIN SanPham sp ON i.MaSP = sp.MaSP
    WHERE NOT EXISTS (
        SELECT 1 
        FROM NhatKyBanHang 
        WHERE MaDH = i.MaDH AND MaSP = i.MaSP
    );
END