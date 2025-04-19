use WebThoiTrang
GO

CREATE OR ALTER TRIGGER trg_CapNhatDoanhThu
ON DonHang
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM inserted WHERE TrangThai = 'dagiao')
        BEGIN
            DECLARE @NgayBaoCao DATE = CAST(GETDATE() AS DATE);
            
            -- Tính toán chi tiết cho đơn hàng mới
            WITH ChiPhiDonHang AS (
                SELECT 
                    i.MaDH,
                    i.TongTienDonHang as DoanhThu,
                    SUM(ct.SoLuongMua * sp.GiaVon) as ChiPhi
                FROM inserted i
                JOIN ChiTietDonHang ct ON i.MaDH = ct.MaDH
                JOIN SanPham sp ON ct.MaSP = sp.MaSP
                WHERE i.TrangThai = 'dagiao'
                GROUP BY i.MaDH, i.TongTienDonHang
            )
            -- Thực hiện INSERT hoặc UPDATE dựa trên kết quả tính toán
            MERGE DoanhThu AS target
            USING (
                SELECT 
                    @NgayBaoCao as NgayBaoCao,
                    COUNT(DISTINCT MaDH) as SoDonHang,
                    SUM(DoanhThu) as TongDoanhThu,
                    SUM(ChiPhi) as TongChiPhi,
                    SUM(DoanhThu) - SUM(ChiPhi) as LoiNhuan
                FROM ChiPhiDonHang
            ) AS source
            ON target.NgayBaoCao = source.NgayBaoCao
            WHEN MATCHED THEN
                UPDATE SET 
                    target.SoDonHang = target.SoDonHang + source.SoDonHang,
                    target.TongDoanhThu = target.TongDoanhThu + source.TongDoanhThu,
                    target.TongChiPhi = target.TongChiPhi + source.TongChiPhi,
                    target.LoiNhuan = target.LoiNhuan + source.LoiNhuan
            WHEN NOT MATCHED THEN
                INSERT (NgayBaoCao, SoDonHang, TongDoanhThu, TongChiPhi, LoiNhuan)
                VALUES (
                    source.NgayBaoCao,
                    source.SoDonHang,
                    source.TongDoanhThu,
                    source.TongChiPhi,
                    source.LoiNhuan
                );
        END
        
        PRINT 'Cập nhật doanh thu thành công'
    END TRY
    BEGIN CATCH
        PRINT 'Lỗi: ' + ERROR_MESSAGE()
    END CATCH
END