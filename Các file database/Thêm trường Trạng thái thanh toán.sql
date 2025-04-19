use WebThoiTrang
GO

-- Thêm trường TrangThaiThanhToan vào bảng DonHang
ALTER TABLE DonHang
ADD TrangThaiThanhToan varchar(20) CHECK (TrangThaiThanhToan IN ('dathanhtoan', 'chuathanhtoan')) DEFAULT 'chuathanhtoan';