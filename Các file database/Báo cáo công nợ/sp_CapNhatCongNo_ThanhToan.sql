use WebThoiTrang
GO

CREATE PROCEDURE sp_CapNhatCongNo_ThanhToan
    @MaDH int,
    @MaTT int,
    @SoTienThanhToan decimal(18,2)
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION

        -- Cập nhật số tiền đã thanh toán trong CongNo
        UPDATE cn
        SET 
            DaThanhToan = DaThanhToan + @SoTienThanhToan,
            ConLai = SoTienNo - (DaThanhToan + @SoTienThanhToan),
            TrangThai = CASE 
                WHEN SoTienNo <= (DaThanhToan + @SoTienThanhToan) THEN N'Đã thanh toán'
                ELSE N'Đang nợ'
            END
        FROM CongNo cn
        JOIN ChiTietCongNo ct ON cn.MaCN = ct.MaCN
        WHERE ct.MaDH = @MaDH AND ct.MaTT = @MaTT;

        -- Thêm chi tiết thanh toán
        INSERT INTO ChiTietCongNo (MaCN, MaDH, MaTT, NgayPhatSinh, SoTien, LoaiPhatSinh, GhiChu)
        SELECT 
            cn.MaCN,
            @MaDH,
            @MaTT,
            GETDATE(),
            @SoTienThanhToan,
            N'Thanh toán',
            N'Thanh toán cho đơn hàng ' + CAST(@MaDH as nvarchar(10))
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