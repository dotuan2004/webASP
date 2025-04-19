use WebThoiTrang
GO

CREATE TRIGGER trg_CapNhatCongNo_TrangThaiDonHang
ON DonHang
AFTER UPDATE
AS
BEGIN
    IF UPDATE(TrangThai)
    BEGIN
        -- Xử lý khi đơn hàng bị hủy
        IF EXISTS (SELECT 1 FROM inserted WHERE TrangThai = 'trahang')
        BEGIN
            DECLARE @MaDH int, @MaTT int
            SELECT @MaDH = MaDH, @MaTT = MaTT FROM inserted
            EXEC sp_XuLyCongNo_HuyDonHang @MaDH, @MaTT
        END
    END
END
