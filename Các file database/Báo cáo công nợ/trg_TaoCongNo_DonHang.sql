use WebThoiTrang
GO

-- DROP TRIGGER trg_TaoCongNo_DonHang

CREATE TRIGGER trg_TaoCongNo_DonHang
ON DonHang
AFTER INSERT
AS
BEGIN
    -- Tạo công nợ từ đơn hàng chưa thanh toán
    INSERT INTO CongNo (MaKH, NgayPhatSinh, SoTienNo, DaThanhToan, ConLai, HanThanhToan, TrangThai)
    SELECT 
        MaKH,
        GETDATE(),
        TongTienDonHang,
        0, -- DaThanhToan ban đầu = 0
        TongTienDonHang, -- ConLai ban đầu = TongTienDonHang
        DATEADD(DAY, 30, GETDATE()), -- Hạn thanh toán 30 ngày
        N'Đang nợ'
    FROM inserted 
    WHERE TrangThaiThanhToan = 'chuathanhtoan';

    -- Tạo chi tiết công nợ cho đơn hàng mới
    INSERT INTO ChiTietCongNo (MaCN, MaDH, MaTT, NgayPhatSinh, SoTien, LoaiPhatSinh, GhiChu)
    SELECT 
        cn.MaCN,
        i.MaDH,
        i.MaTT,
        GETDATE(),
        i.TongTienDonHang,
        N'Nợ mới từ đơn hàng',
        N'Công nợ từ đơn hàng ' + CAST(i.MaDH as nvarchar(10))
    FROM inserted i
    JOIN CongNo cn ON cn.MaKH = i.MaKH 
    WHERE cn.NgayPhatSinh >= DATEADD(second, -5, GETDATE()) -- Lấy công nợ vừa được tạo
    AND i.TrangThaiThanhToan = 'chuathanhtoan';
END