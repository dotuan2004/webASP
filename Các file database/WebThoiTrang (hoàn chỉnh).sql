-- Tạo Database
CREATE DATABASE WebThoiTrang
GO
USE WebThoiTrang
GO

-- Tạo các bảng
Create table [Role]
(
	[RoleID] Integer NOT NULL IDENTITY,
	[RoleName] Nvarchar(20) NULL,
Primary Key ([RoleID])
) 
GO

Create table [KhachHang]
(
	[MaKH] Integer NOT NULL IDENTITY,
	[UserName] Varchar(20) NOT NULL,
	[Password] Varchar(20) NOT NULL,
	[TenKhachHang] Nvarchar(100) NOT NULL,
	[Email] Nvarchar(100) NOT NULL,
	[SDT]  Varchar(11) NOT NULL,
	[DiaChi] Nvarchar(100) NOT NULL,
	[RoleID] Integer NOT NULL,
Primary Key ([MaKH])
) 
GO

Create table [Loai]
(
	[MaLoai] Integer NOT NULL IDENTITY,
	[TenLoai] Nvarchar(100) NOT NULL,
	[Hinh] Nvarchar (100) NOT NULL,
Primary Key ([MaLoai])
) 
GO

Create table [NCC]
(
	[MaNCC] Integer NOT NULL IDENTITY,
	[TenNCC] Nvarchar(50) NOT NULL,
	[Email] Nvarchar(50) NOT NULL,
	[SDT] Varchar(11) NOT NULL,
	[Diachi] Nvarchar(50) NOT NULL,
Primary Key ([MaNCC])
) 
GO

Create table [SanPham]
(
	[MaSP] Integer NOT NULL IDENTITY,
	[TenSP] Nvarchar(200) NOT NULL,
	[Hinh] Nvarchar(100) NULL,
	[GiaSP] Integer NOT NULL CHECK ([GiaSP]>0),
	[GiaVon] Integer NOT NULL CHECK ([GiaVon]>0),
	[SoLuongTon] Integer NOT NULL CHECK ([SoLuongTon]>=0),
	[MoTa] Nvarchar(1000) NULL,
	[MaLoai] Integer NOT NULL,
	[MaNCC] Integer NOT NULL,
	[TrangThai] bit DEFAULT 1,
Primary Key ([MaSP])
) 
GO

Create table [ThanhToan]
(
	[MaTT] Integer NOT NULL IDENTITY,
	[TenTT] Nvarchar(50) NOT NULL,
Primary Key ([MaTT])
) 
GO

Create table [DonHang]
(
	[MaDH] Integer NOT NULL IDENTITY,
	[NgayDatHang] Datetime NOT NULL DEFAULT GETDATE(),
	[NgayGiaoHangDuKien] Datetime NOT NULL DEFAULT GETDATE(),
	[TongTienDonHang] Decimal(10,2) NOT NULL,
	[GhiChu] NVARCHAR(MAX),
    [ChietKhau] DECIMAL(18,2),
    [PhiVanChuyen] DECIMAL(18,2),
    [DiaChiGiao] NVARCHAR(255),
	[ThueVAT] DECIMAL(5,2) DEFAULT 0,  -- Phần trăm VAT (VD: 10.00 là 10%)
    [TienThueVAT] DECIMAL(18,2) DEFAULT 0, -- Số tiền thuế VAT
    [ThanhTienTruocVAT] DECIMAL(18,2) DEFAULT 0, -- Tổng tiền trước thuế
    [ThanhTienSauVAT] DECIMAL(18,2) DEFAULT 0, -- Tổng tiền sau thuế (bao gồm VAT)
	[TrangThai] Varchar(20) NOT NULL CHECK ([TrangThai]='dagiao' OR [TrangThai]='chuagiao' OR [TrangThai]='trahang'),
	[MaKH] Integer NOT NULL,
	[MaTT] Integer NOT NULL,
	[NgayTao] datetime DEFAULT GETDATE(),
Primary Key ([MaDH],[MaTT])
) 
GO

Create table [ChiTietDonHang]
(
	[MaCTDH] Integer NOT NULL IDENTITY,
	[MaDH] Integer NOT NULL,
	[MaTT] Integer NOT NULL,
	[MaSP] Integer NOT NULL,
	[SoLuongMua] Integer NOT NULL,
	[TongTien] Decimal(10,2) NULL,
Primary Key ([MaCTDH],[MaDH],[MaTT],[MaSP])
) 
GO

Create table [Backup_History]
(
    [BackupID] INT IDENTITY(1,1) PRIMARY KEY,
    [BackupName] NVARCHAR(100) NOT NULL,
    [BackupDate] DATETIME DEFAULT GETDATE(),
    [BackupPath] NVARCHAR(500) NOT NULL,
    [Description] NVARCHAR(500)
)
GO

Create table [BaoCao]
(
    [MaBaoCao] INT IDENTITY(1,1) PRIMARY KEY,
    [LoaiBaoCao] NVARCHAR(50) CHECK ([LoaiBaoCao] IN ('BaoCaoLuuChuyenTienTe', 'BangCanDoiKeToan', 'BaoCaoKetQuaKinhDoanh')),
    [ThoiGianBatDau] DATE,
    [ThoiGianKetThuc] DATE,
    [NgayLap] DATETIME DEFAULT GETDATE(),
    [NguoiLap] INT FOREIGN KEY REFERENCES KhachHang(MaKH),
    [TongDoanhThu] DECIMAL(15,2),
    [TongChiPhi] DECIMAL(15,2),
    [LoiNhuan] DECIMAL(15,2),
    [GhiChu] NVARCHAR(500)
)
GO

-- Bảng Khuyến Mãi
CREATE TABLE [KhuyenMai] (
    [MaKM] Integer NOT NULL IDENTITY,  
    [TenKM] Nvarchar(200) NOT NULL,
    [MoTa] Nvarchar(500),
    [NgayBatDau] Datetime NOT NULL,
    [NgayKetThuc] Datetime NOT NULL,
    [PhanTramGiam] Float NOT NULL CHECK ([PhanTramGiam] > 0 AND [PhanTramGiam] <= 100),
    [DieuKienApDung] Decimal(10,2),  -- Giá trị đơn hàng tối thiểu
    [TrangThai] bit DEFAULT 1,
    PRIMARY KEY ([MaKM])
)

-- Bảng SanPham_KhuyenMai (quan hệ n-n giữa SP và KM)
CREATE TABLE [SanPham_KhuyenMai] (
    [MaSP] Integer NOT NULL,
    [MaKM] Integer NOT NULL,
    [NgayApDung] Datetime DEFAULT GETDATE(),
    PRIMARY KEY ([MaSP], [MaKM]),
    FOREIGN KEY ([MaSP]) REFERENCES [SanPham]([MaSP]),
    FOREIGN KEY ([MaKM]) REFERENCES [KhuyenMai]([MaKM])
)

-- Bảng Lịch sử giá
CREATE TABLE [LichSuGia] (
    [MaLSG] Integer NOT NULL IDENTITY,
    [MaSP] Integer NOT NULL,
    [GiaCu] Decimal(10,2) NOT NULL,
    [GiaMoi] Decimal(10,2) NOT NULL,
    [NgayCapNhat] Datetime DEFAULT GETDATE(),
    [GhiChu] Nvarchar(200),
    PRIMARY KEY ([MaLSG]),
    FOREIGN KEY ([MaSP]) REFERENCES [SanPham]([MaSP])
)

-- Bảng Nhập kho
CREATE TABLE [PhieuNhap] (
    [MaPN] Integer NOT NULL IDENTITY,
    [NgayNhap] Datetime DEFAULT GETDATE(),
    [MaNCC] Integer NOT NULL,
    [TongTien] Decimal(10,2) NOT NULL,
    [GhiChu] Nvarchar(500),
    [NguoiNhap] Integer NOT NULL,  -- FK to KhachHang (nhân viên)
    PRIMARY KEY ([MaPN]),
    FOREIGN KEY ([MaNCC]) REFERENCES [NCC]([MaNCC]),
    FOREIGN KEY ([NguoiNhap]) REFERENCES [KhachHang]([MaKH])
)

-- Chi tiết phiếu nhập
CREATE TABLE [ChiTietPhieuNhap] (
    [MaCTPN] Integer NOT NULL IDENTITY,
    [MaPN] Integer NOT NULL,
    [MaSP] Integer NOT NULL,
    [SoLuong] Integer NOT NULL CHECK ([SoLuong] > 0),
    [DonGia] Decimal(10,2) NOT NULL,
    [ThanhTien] Decimal(10,2) NOT NULL,
    PRIMARY KEY ([MaCTPN]),
    FOREIGN KEY ([MaPN]) REFERENCES [PhieuNhap]([MaPN]),
    FOREIGN KEY ([MaSP]) REFERENCES [SanPham]([MaSP])
)

-- Bảng Xuất kho
CREATE TABLE [PhieuXuat] (
    [MaPX] Integer NOT NULL IDENTITY,
    [NgayXuat] Datetime DEFAULT GETDATE(),
    [LyDo] Nvarchar(200),  -- Bán hàng, hủy hàng, trả hàng...
    [TongTien] Decimal(10,2) NOT NULL,	
    [GhiChu] Nvarchar(500),
    [NguoiXuat] Integer NOT NULL,  -- FK to KhachHang (nhân viên)
    PRIMARY KEY ([MaPX]),
    FOREIGN KEY ([NguoiXuat]) REFERENCES [KhachHang]([MaKH])
)

-- Chi tiết phiếu xuất
CREATE TABLE [ChiTietPhieuXuat] (
    [MaCTPX] Integer NOT NULL IDENTITY,
    [MaPX] Integer NOT NULL,
    [MaSP] Integer NOT NULL,
    [SoLuong] Integer NOT NULL CHECK ([SoLuong] > 0),
    [DonGia] Decimal(10,2) NOT NULL,
    [ThanhTien] Decimal(10,2) NOT NULL,
    PRIMARY KEY ([MaCTPX]),
    FOREIGN KEY ([MaPX]) REFERENCES [PhieuXuat]([MaPX]),
    FOREIGN KEY ([MaSP]) REFERENCES [SanPham]([MaSP])
)

-- Bảng Yêu thích
CREATE TABLE [YeuThich] (
    [MaKH] Integer NOT NULL,
    [MaSP] Integer NOT NULL,
    [NgayThem] Datetime DEFAULT GETDATE(),
    PRIMARY KEY ([MaKH], [MaSP]),
    FOREIGN KEY ([MaKH]) REFERENCES [KhachHang]([MaKH]),
    FOREIGN KEY ([MaSP]) REFERENCES [SanPham]([MaSP])
)

-- Bảng Nhật ký bán hàng (chi tiết từng giao dịch)
CREATE TABLE [NhatKyBanHang] (
    [MaNK] Integer NOT NULL IDENTITY,
    [NgayGiaoDich] Datetime DEFAULT GETDATE(),
    [MaDH] Integer NOT NULL,
    [MaTT] Integer NOT NULL,
    [MaSP] Integer NOT NULL,
    [SoLuong] Integer NOT NULL,
    [DonGia] Decimal(10,2) NOT NULL,
    [ThanhTien] Decimal(10,2) NOT NULL,
    [TrangThai] Nvarchar(50),
    PRIMARY KEY ([MaNK]),
    FOREIGN KEY ([MaDH], [MaTT]) REFERENCES [DonHang]([MaDH], [MaTT]),
    FOREIGN KEY ([MaSP]) REFERENCES [SanPham]([MaSP])
)

-- Bảng Doanh thu (tổng hợp theo ngày/tháng)
CREATE TABLE [DoanhThu] (
    [MaDT] Integer NOT NULL IDENTITY,
    [NgayBaoCao] Date NOT NULL,
    [SoDonHang] Integer NOT NULL DEFAULT 0,
    [TongDoanhThu] Decimal(15,2) NOT NULL DEFAULT 0,
    [TongChiPhi] Decimal(15,2) NOT NULL DEFAULT 0,
    [LoiNhuan] Decimal(15,2) NOT NULL DEFAULT 0,
    [GhiChu] Nvarchar(500),
    PRIMARY KEY ([MaDT])
)

-- Bảng Công nợ
CREATE TABLE [CongNo] (
    [MaCN] Integer NOT NULL IDENTITY,
    [MaKH] Integer NOT NULL,
    [NgayPhatSinh] Datetime DEFAULT GETDATE(),
    [SoTienNo] Decimal(15,2) NOT NULL DEFAULT 0,
    [DaThanhToan] Decimal(15,2) NOT NULL DEFAULT 0,
    [ConLai] Decimal(15,2) NOT NULL DEFAULT 0,
    [HanThanhToan] Date,
    [TrangThai] Nvarchar(50),
    PRIMARY KEY ([MaCN]),
    FOREIGN KEY ([MaKH]) REFERENCES [KhachHang]([MaKH])
)

-- Bảng Chi tiết công nợ
CREATE TABLE [ChiTietCongNo] (
    [MaCTCN] Integer NOT NULL IDENTITY,
    [MaCN] Integer NOT NULL,
    [MaDH] Integer,  -- Có thể NULL nếu là khoản nợ khác
    [MaTT] Integer,
    [NgayPhatSinh] Datetime DEFAULT GETDATE(),
    [SoTien] Decimal(15,2) NOT NULL,
    [LoaiPhatSinh] Nvarchar(50),  -- Nợ mới, thanh toán
    [GhiChu] Nvarchar(500),
    PRIMARY KEY ([MaCTCN]),
    FOREIGN KEY ([MaCN]) REFERENCES [CongNo]([MaCN]),
    FOREIGN KEY ([MaDH], [MaTT]) REFERENCES [DonHang]([MaDH], [MaTT])
)

-- Bảng Báo cáo tồn kho
CREATE TABLE [BaoCaoTonKho] (
    [MaBCTK] Integer NOT NULL IDENTITY,
    [NgayBaoCao] Date NOT NULL,
    [MaSP] Integer NOT NULL,
    [TonDau] Integer NOT NULL DEFAULT 0,
    [NhapTrongKy] Integer NOT NULL DEFAULT 0,
    [XuatTrongKy] Integer NOT NULL DEFAULT 0,
    [TonCuoi] Integer NOT NULL DEFAULT 0,
    [GiaTriTonKho] Decimal(15,2) NOT NULL DEFAULT 0,
    PRIMARY KEY ([MaBCTK]),
    FOREIGN KEY ([MaSP]) REFERENCES [SanPham]([MaSP])
)

-- Bảng Giỏ hàng
CREATE TABLE [GioHang]
(
    [MaGioHang] Integer NOT NULL IDENTITY,
    [MaKH] Integer NOT NULL,
    [NgayTao] Datetime DEFAULT GETDATE(),
    [NgayCapNhat] Datetime DEFAULT GETDATE(),
    [TongTien] Decimal(10,2) DEFAULT 0,
    [TrangThai] bit DEFAULT 1,  -- 1: Active, 0: Converted to Order
    PRIMARY KEY ([MaGioHang]),
    FOREIGN KEY ([MaKH]) REFERENCES [KhachHang]([MaKH])
)
GO

-- Bảng Chi tiết giỏ hàng
CREATE TABLE [ChiTietGioHang]
(
    [MaCTGH] Integer NOT NULL IDENTITY,
    [MaGioHang] Integer NOT NULL,
    [MaSP] Integer NOT NULL,
    [SoLuong] Integer NOT NULL CHECK ([SoLuong] > 0),
    [DonGia] Decimal(10,2) NOT NULL,
    [ThanhTien] Decimal(10,2) NOT NULL,
    [NgayThem] Datetime DEFAULT GETDATE(),
    PRIMARY KEY ([MaCTGH]),
    FOREIGN KEY ([MaGioHang]) REFERENCES [GioHang]([MaGioHang]),
    FOREIGN KEY ([MaSP]) REFERENCES [SanPham]([MaSP])
)
GO

-- Tạo các ràng buộc khóa ngoại
Alter table [KhachHang] add foreign key([RoleID]) references [Role] ([RoleID])
GO

Alter table [SanPham] add foreign key([MaLoai]) references [Loai] ([MaLoai])
GO

Alter table [SanPham] add foreign key([MaNCC]) references [NCC] ([MaNCC])
GO

Alter table [DonHang] add foreign key([MaKH]) references [KhachHang] ([MaKH])
GO

Alter table [DonHang] add foreign key([MaTT]) references [ThanhToan] ([MaTT])
GO

Alter table [ChiTietDonHang] add foreign key([MaSP]) references [SanPham] ([MaSP])
GO

Alter table [ChiTietDonHang] add foreign key([MaDH],[MaTT]) references [DonHang] ([MaDH],[MaTT])
GO


-- Thêm dữ liệu mẫu
-- 1. Thêm Role
INSERT INTO Role (RoleName) VALUES 
(N'Admin'),
(N'Khách hàng'),
(N'Nhân viên');

-- 2. Thêm KhachHang
INSERT INTO KhachHang (UserName, Password, TenKhachHang, Email, SDT, DiaChi, RoleID) VALUES
('admin', '123456', N'Nguyễn Admin', 'admin@gmail.com', '0901234567', N'123 Nguyễn Huệ, Q1, TP.HCM', 1),
('nhanvien1', '123456', N'Trần Văn A', 'trana@gmail.com', '0912345678', N'456 Lê Lợi, Q1, TP.HCM', 3),
('khachhang1', '123456', N'Phạm Thị B', 'phamb@gmail.com', '0923456789', N'789 Võ Văn Tần, Q3, TP.HCM', 2);

-- 3. Thêm Loai
INSERT INTO Loai (TenLoai, Hinh) VALUES
(N'Váy Đầm', '/Content/images/vaydam.jpg'),
(N'Áo Vest nữ', '/Content/images/aovest.jpg'),
(N'Chân váy', '/Content/images/chanvay.jpg'),
(N'Quần nữ', '/Content/images/quannu.jpg'),
(N'Áo nữ', '/Content/images/aonu.jpg'),
(N'Áo phông', '/Content/images/aophong.jpg'),
(N'Set Đồ Nữ', '/Content/images/setdonu.jpg'),
(N'Áo Dài', '/Content/images/aodai.jpg'),
(N'Măng Tô', '/Content/images/mangto.jpg');

-- 4. Thêm NCC
INSERT INTO NCC (TenNCC, Email, SDT, Diachi) VALUES
(N'Công ty May Việt Tiến', 'viettien@gmail.com', '0928888888', N'Quận Tân Bình, TP.HCM'),
(N'Công ty Dệt may Phong Phú', 'phongphu@gmail.com', '0929999999', N'Quận 9, TP.HCM');

-- 5. Thêm SanPham
INSERT INTO SanPham (TenSP, Hinh, GiaSP, GiaVon, SoLuongTon, MoTa, MaLoai, MaNCC, TrangThai) VALUES
-- Váy đầm (MaLoai = 1)
(N'Váy đầm dự tiệc cao cấp', '/Content/images/vaydam1.jpg', 850000, 600000, 30, N'Váy đầm dự tiệc chất liệu vải cao cấp, kiểu dáng sang trọng', 1, 1, 1),
(N'Váy đầm công sở thanh lịch', '/Content/images/vaydam2.jpg', 650000, 450000, 45, N'Váy đầm công sở form suông thanh lịch', 1, 2, 1),
(N'Váy đầm dạ hội sang trọng', '/Content/images/vaydam3.jpg', 1200000, 800000, 20, N'Váy đầm dạ hội dáng xòe, đính kết cao cấp', 1, 1, 1),

-- Áo vest nữ (MaLoai = 2)
(N'Áo vest nữ công sở', '/Content/images/aovest1.jpg', 750000, 500000, 40, N'Áo vest nữ form chuẩn, phong cách hiện đại', 2, 1, 1),
(N'Áo vest nữ dáng croptop', '/Content/images/aovest2.jpg', 680000, 450000, 35, N'Áo vest nữ dáng ngắn thời trang', 2, 2, 1),

-- Chân váy (MaLoai = 3)
(N'Chân váy xếp ly', '/Content/images/chanvay1.jpg', 450000, 300000, 50, N'Chân váy xếp ly dáng dài thanh lịch', 3, 1, 1),
(N'Chân váy bút chì', '/Content/images/chanvay2.jpg', 420000, 280000, 45, N'Chân váy bút chì công sở', 3, 2, 1),

-- Quần nữ (MaLoai = 4)
(N'Quần tây nữ công sở', '/Content/images/quannu1.jpg', 480000, 320000, 60, N'Quần tây nữ form chuẩn công sở', 4, 2, 1),
(N'Quần ống rộng nữ', '/Content/images/quannu2.jpg', 500000, 350000, 40, N'Quần ống rộng nữ thời trang', 4, 1, 1),

-- Áo nữ (MaLoai = 5)
(N'Áo sơ mi nữ trơn', '/Content/images/aonu1.jpg', 350000, 230000, 70, N'Áo sơ mi nữ kiểu dáng cơ bản', 5, 1, 1),
(N'Áo kiểu nữ công sở', '/Content/images/aonu2.jpg', 380000, 250000, 65, N'Áo kiểu nữ thiết kế hiện đại', 5, 2, 1),

-- Áo phông (MaLoai = 6)
(N'Áo phông form rộng', '/Content/images/aophong1.jpg', 280000, 180000, 100, N'Áo phông form rộng phong cách', 6, 1, 1),
(N'Áo phông in họa tiết', '/Content/images/aophong2.jpg', 300000, 200000, 90, N'Áo phông in họa tiết trendy', 6, 2, 1),

-- Set đồ nữ (MaLoai = 7)
(N'Set đồ công sở', '/Content/images/setdo1.jpg', 850000, 600000, 25, N'Set đồ công sở cao cấp', 7, 1, 1),
(N'Set đồ thể thao', '/Content/images/setdo2.jpg', 750000, 500000, 30, N'Set đồ thể thao năng động', 7, 2, 1),

-- Áo dài (MaLoai = 8)
(N'Áo dài truyền thống', '/Content/images/aodai1.jpg', 1200000, 800000, 20, N'Áo dài truyền thống chất liệu lụa', 8, 1, 1),
(N'Áo dài cách tân', '/Content/images/aodai2.jpg', 950000, 650000, 25, N'Áo dài cách tân hiện đại', 8, 2, 1),

-- Măng tô (MaLoai = 9)
(N'Măng tô dạ dài', '/Content/images/mangto1.jpg', 1500000, 1000000, 15, N'Măng tô dạ dài cao cấp', 9, 1, 1),
(N'Măng tô ngắn', '/Content/images/mangto2.jpg', 1200000, 800000, 20, N'Măng tô ngắn thời trang', 9, 2, 1);

-- 6. Thêm ThanhToan
INSERT INTO ThanhToan (TenTT) VALUES
(N'Thanh toán khi nhận hàng'),
(N'Chuyển khoản ngân hàng'),
(N'Ví điện tử');

-- 7. Thêm DonHang
INSERT INTO DonHang (NgayDatHang, NgayGiaoHangDuKien, TongTienDonHang, TrangThai, MaKH, MaTT) VALUES
('2024-01-01', '2024-01-03', 150000, 'dagiao', 3, 1),
('2024-01-02', '2024-01-04', 450000, 'chuagiao', 3, 2),
('2024-01-03', '2024-01-05', 850000, 'dagiao', 3, 3);

-- 8. Thêm ChiTietDonHang
INSERT INTO ChiTietDonHang (MaDH, MaTT, MaSP, SoLuongMua, TongTien) VALUES
(1, 1, 1, 1, 150000),
(2, 2, 4, 1, 450000),
(3, 3, 7, 1, 850000);

-- 9. Thêm Backup_History
INSERT INTO Backup_History (BackupName, BackupPath, Description) VALUES
(N'Backup_Full_012024', 'D:\Backups\Full_012024.bak', N'Backup toàn bộ dữ liệu tháng 1/2024'),
(N'Backup_Full_022024', 'D:\Backups\Full_022024.bak', N'Backup toàn bộ dữ liệu tháng 2/2024');

-- Thêm dữ liệu mẫu cho bảng BaoCao
INSERT INTO BaoCao (LoaiBaoCao, ThoiGianBatDau, ThoiGianKetThuc, NgayLap, NguoiLap, TongDoanhThu, TongChiPhi, LoiNhuan, GhiChu) 
VALUES 
-- Báo cáo lưu chuyển tiền tệ
(N'BaoCaoLuuChuyenTienTe', '2024-01-01', '2024-01-31', '2024-02-01', 1, 125000000.00, 85000000.00, 40000000.00, N'Báo cáo lưu chuyển tiền tệ tháng 1/2024'),
(N'BaoCaoLuuChuyenTienTe', '2024-02-01', '2024-02-29', '2024-03-01', 1, 145000000.00, 98000000.00, 47000000.00, N'Báo cáo lưu chuyển tiền tệ tháng 2/2024'),

-- Bảng cân đối kế toán
(N'BangCanDoiKeToan', '2024-01-01', '2024-01-31', '2024-02-01', 1, 125000000.00, 85000000.00, 40000000.00, N'Bảng cân đối kế toán tháng 1/2024'),
(N'BangCanDoiKeToan', '2024-02-01', '2024-02-29', '2024-03-01', 1, 145000000.00, 98000000.00, 47000000.00, N'Bảng cân đối kế toán tháng 2/2024'),

-- Báo cáo kết quả kinh doanh
(N'BaoCaoKetQuaKinhDoanh', '2024-01-01', '2024-01-31', '2024-02-01', 1, 125000000.00, 85000000.00, 40000000.00, N'Báo cáo kết quả kinh doanh tháng 1/2024'),
(N'BaoCaoKetQuaKinhDoanh', '2024-02-01', '2024-02-29', '2024-03-01', 1, 145000000.00, 98000000.00, 47000000.00, N'Báo cáo kết quả kinh doanh tháng 2/2024');

-- Thêm dữ liệu mẫu cho KhuyenMai
INSERT INTO KhuyenMai (TenKM, MoTa, NgayBatDau, NgayKetThuc, PhanTramGiam, DieuKienApDung) VALUES
(N'Khuyến mãi đầu năm', N'Giảm giá đầu năm 2024', '2024-01-01', '2024-01-31', 20, 1000000),
(N'Khuyến mãi tháng 2', N'Giảm giá tháng 2/2024', '2024-02-01', '2024-02-29', 15, 500000);

-- Thêm dữ liệu mẫu cho SanPham_KhuyenMai
INSERT INTO SanPham_KhuyenMai (MaSP, MaKM) VALUES
(1, 1),
(2, 1),
(3, 2);

-- Thêm dữ liệu mẫu cho LichSuGia
INSERT INTO LichSuGia (MaSP, GiaCu, GiaMoi, GhiChu) VALUES
(1, 800000, 850000, N'Tăng giá do chi phí vận chuyển'),
(2, 600000, 650000, N'Điều chỉnh giá theo thị trường');

-- Thêm dữ liệu mẫu cho PhieuNhap và ChiTietPhieuNhap
INSERT INTO PhieuNhap (NgayNhap, MaNCC, TongTien, NguoiNhap) VALUES
('2024-01-15', 1, 5000000, 2);

INSERT INTO ChiTietPhieuNhap (MaPN, MaSP, SoLuong, DonGia, ThanhTien) VALUES
(1, 1, 10, 500000, 5000000);

-- Thêm dữ liệu mẫu cho NhatKyBanHang
INSERT INTO NhatKyBanHang (NgayGiaoDich, MaDH, MaTT, MaSP, SoLuong, DonGia, ThanhTien, TrangThai) VALUES
('2024-01-20', 1, 1, 1, 1, 850000, 850000, N'Hoàn thành');

-- Thêm dữ liệu mẫu cho DoanhThu
INSERT INTO DoanhThu (NgayBaoCao, SoDonHang, TongDoanhThu, TongChiPhi, LoiNhuan) VALUES
('2024-01-31', 10, 8500000, 5000000, 3500000);

-- Thêm dữ liệu mẫu cho CongNo
INSERT INTO CongNo (MaKH, SoTienNo, DaThanhToan, ConLai, HanThanhToan, TrangThai) VALUES
(3, 1000000, 500000, 500000, '2024-02-29', N'Đang nợ');

-- Thêm dữ liệu mẫu
INSERT INTO GioHang (MaKH) VALUES
(3);  -- Giỏ hàng cho khách hàng có MaKH = 3

INSERT INTO ChiTietGioHang (MaGioHang, MaSP, SoLuong, DonGia, ThanhTien) VALUES
(1, 1, 2, 850000, 1700000),
(1, 2, 1, 650000, 650000);