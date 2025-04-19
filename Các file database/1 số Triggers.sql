use WebThoiTrang
GO

-- Trigger cập nhật đơn hàng
CREATE TRIGGER trg_UpdateDonHang
ON ChiTietDonHang
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Kiểm tra số lượng tồn
    IF EXISTS (
        SELECT 1 
        FROM inserted i
        JOIN SanPham sp ON i.MaSP = sp.MaSP
        WHERE i.SoLuongMua > sp.SoLuongTon
    )
    BEGIN
        RAISERROR (N'Số lượng sản phẩm trong kho không đủ!', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- Cập nhật số lượng tồn kho
    UPDATE sp
    SET sp.SoLuongTon = sp.SoLuongTon - i.SoLuongMua
    FROM SanPham sp
    JOIN inserted i ON sp.MaSP = i.MaSP;

    -- Cập nhật tổng tiền đơn hàng (có tính khuyến mãi)
    UPDATE dh
    SET 
        ThanhTienTruocVAT = (
            SELECT SUM(ctdh.SoLuongMua * 
                      (sp.GiaSP * (1 - ISNULL(km.PhanTramGiam, 0)/100)))
            FROM ChiTietDonHang ctdh
            JOIN SanPham sp ON ctdh.MaSP = sp.MaSP
            LEFT JOIN SanPham_KhuyenMai spkm ON sp.MaSP = spkm.MaSP
            LEFT JOIN KhuyenMai km ON spkm.MaKM = km.MaKM
                AND GETDATE() BETWEEN km.NgayBatDau AND km.NgayKetThuc
            WHERE ctdh.MaDH = dh.MaDH
        ),
        TienThueVAT = (
            SELECT SUM(ctdh.SoLuongMua * 
                      (sp.GiaSP * (1 - ISNULL(km.PhanTramGiam, 0)/100)))
            FROM ChiTietDonHang ctdh
            JOIN SanPham sp ON ctdh.MaSP = sp.MaSP
            LEFT JOIN SanPham_KhuyenMai spkm ON sp.MaSP = spkm.MaSP
            LEFT JOIN KhuyenMai km ON spkm.MaKM = km.MaKM
                AND GETDATE() BETWEEN km.NgayBatDau AND km.NgayKetThuc
            WHERE ctdh.MaDH = dh.MaDH
        ) * (ThueVAT / 100),
        ThanhTienSauVAT = (
            SELECT SUM(ctdh.SoLuongMua * 
                      (sp.GiaSP * (1 - ISNULL(km.PhanTramGiam, 0)/100)))
            FROM ChiTietDonHang ctdh
            JOIN SanPham sp ON ctdh.MaSP = sp.MaSP
            LEFT JOIN SanPham_KhuyenMai spkm ON sp.MaSP = spkm.MaSP
            LEFT JOIN KhuyenMai km ON spkm.MaKM = km.MaKM
                AND GETDATE() BETWEEN km.NgayBatDau AND km.NgayKetThuc
            WHERE ctdh.MaDH = dh.MaDH
        ) * (1 + ThueVAT / 100),
        TongTienDonHang = (
            SELECT SUM(ctdh.SoLuongMua * 
                      (sp.GiaSP * (1 - ISNULL(km.PhanTramGiam, 0)/100))) * (1 + ThueVAT / 100)
            FROM ChiTietDonHang ctdh
            JOIN SanPham sp ON ctdh.MaSP = sp.MaSP
            LEFT JOIN SanPham_KhuyenMai spkm ON sp.MaSP = spkm.MaSP
            LEFT JOIN KhuyenMai km ON spkm.MaKM = km.MaKM
                AND GETDATE() BETWEEN km.NgayBatDau AND km.NgayKetThuc
            WHERE ctdh.MaDH = dh.MaDH
        )
    FROM DonHang dh
    WHERE EXISTS (
        SELECT 1 FROM inserted i WHERE i.MaDH = dh.MaDH
    ) OR EXISTS (
        SELECT 1 FROM deleted d WHERE d.MaDH = dh.MaDH
    );
END;

-- Trigger cập nhật tồn kho khi nhập hàng
CREATE TRIGGER trg_NhapHang_UpdateTonKho
ON ChiTietPhieuNhap
AFTER INSERT
AS
BEGIN
    UPDATE sp
    SET sp.SoLuongTon = sp.SoLuongTon + i.SoLuong
    FROM SanPham sp
    JOIN inserted i ON sp.MaSP = i.MaSP
END
GO

-- Trigger cập nhật lịch sử giá khi thay đổi giá sản phẩm
CREATE TRIGGER trg_UpdateGiaSanPham
ON SanPham
AFTER UPDATE
AS
BEGIN
    IF UPDATE(GiaSP)
    BEGIN
        INSERT INTO LichSuGia (MaSP, GiaCu, GiaMoi, NgayCapNhat)
        SELECT 
            i.MaSP,
            d.GiaSP,
            i.GiaSP,
            GETDATE()
        FROM deleted d
        JOIN inserted i ON d.MaSP = i.MaSP
        WHERE d.GiaSP != i.GiaSP
    END
END
GO

-- Trigger tự động cập nhật tổng tiền giỏ hàng
CREATE TRIGGER trg_UpdateGioHangTotal
ON ChiTietGioHang
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    -- Cập nhật tổng tiền và ngày cập nhật cho giỏ hàng
    UPDATE gh
    SET 
        TongTien = (
            SELECT ISNULL(SUM(ThanhTien), 0)
            FROM ChiTietGioHang
            WHERE MaGioHang = gh.MaGioHang
        ),
        NgayCapNhat = GETDATE()
    FROM GioHang gh
    WHERE gh.MaGioHang IN (
        SELECT DISTINCT MaGioHang 
        FROM inserted
        UNION
        SELECT DISTINCT MaGioHang 
        FROM deleted
    )
END
GO