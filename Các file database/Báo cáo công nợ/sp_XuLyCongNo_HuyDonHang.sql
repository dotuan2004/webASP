use WebThoiTrang
GO

CREATE PROCEDURE sp_XuLyCongNo_HuyDonHang
    @MaDH int,
    @MaTT int
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION

        -- Cập nhật trạng thái công nợ
        UPDATE cn
        SET 
            TrangThai = N'Đã hủy',
            ConLai = 0
        FROM CongNo cn
        JOIN ChiTietCongNo ct ON cn.MaCN = ct.MaCN
        WHERE ct.MaDH = @MaDH AND ct.MaTT = @MaTT;

        -- Thêm chi tiết hủy đơn hàng
        INSERT INTO ChiTietCongNo (MaCN, MaDH, MaTT, NgayPhatSinh, SoTien, LoaiPhatSinh, GhiChu)
        SELECT 
            cn.MaCN,
            @MaDH,
            @MaTT,
            GETDATE(),
            0,
            N'Hủy đơn hàng',
            N'Hủy đơn hàng ' + CAST(@MaDH as nvarchar(10))
        FROM CongNo cn
        JOIN ChiTietCongNo ct ON cn.MaCN = ct.MaCN
        WHERE ct.MaDH = @MaDH AND ct.MaTT = @MaTT;

        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        THROW;
    END CATCH
END