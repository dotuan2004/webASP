use WebThoiTrang
GO

-- Thêm trường DaThanhToan vào bảng DonHang
ALTER TABLE DonHang
ADD DaThanhToan Decimal(18,2) DEFAULT 0