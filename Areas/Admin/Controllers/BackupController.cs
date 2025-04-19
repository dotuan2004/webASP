using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Windows;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Wordprocessing;
using LTW.Models;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace LTW.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class BackupController : Controller
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["WebThoiTrangConnectionString"].ConnectionString;

        public ActionResult Index()
        {
            try
            {
                // Lấy danh sách các file backup trong thư mục
                string backupPath = Server.MapPath("~/MyDatabase/Backups");
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }

                var files = Directory.GetFiles(backupPath, "*.bak")
                                   .Select(f => new FileInfo(f))
                                   .OrderByDescending(f => f.LastWriteTime)
                                   .ToList();

                ViewBag.BackupFiles = files;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi tải danh sách backup: " + ex.Message;
            }

            return View();
        }

        [HttpPost]
        public ActionResult Backup()
        {
            try
            {
                string backupPath = Server.MapPath("~/MyDatabase/Backups");
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }

                string fileName = $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                string filePath = Path.Combine(backupPath, fileName);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Backup toàn bộ database với đầy đủ thông tin
                    string query = $@"BACKUP DATABASE [WebThoiTrang] 
                TO DISK = N'{filePath}' 
                WITH 
                COPY_ONLY,                  -- Backup độc lập không ảnh hưởng đến chuỗi backup
                FORMAT,                     -- Tạo media set mới
                INIT,                       -- Ghi đè file nếu tồn tại
                NAME = N'WebThoiTrang-Full Database Backup', 
                SKIP,                       -- Kiểm tra tính hợp lệ của backup
                STATS = 10,                 -- Hiển thị tiến trình
                CHECKSUM,                   -- Kiểm tra tính toàn vẹn
                CONTINUE_AFTER_ERROR; --Tiếp tục nếu có lỗi nhỏ"; 
        
            using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.CommandTimeout = 600; // Tăng timeout lên 10 phút
                        command.ExecuteNonQuery();
                    }

                    // Kiểm tra kích thước file backup
                    var fileInfo = new FileInfo(filePath);
                    if (fileInfo.Length < 5 * 1024 * 1024) // Nếu nhỏ hơn 5MB
                    {
                        throw new Exception("Kích thước file backup quá nhỏ, có thể backup chưa đầy đủ!");
                    }

                    // Lưu thông tin backup
                    string insertHistoryQuery = @"
                INSERT INTO [dbo].[Backup_History]
                ([BackupName], [BackupPath], [Description])
                VALUES (@Name, @Path, @Description)";

                    using (SqlCommand cmd = new SqlCommand(insertHistoryQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Name", fileName);
                        cmd.Parameters.AddWithValue("@Path", filePath);
                        cmd.Parameters.AddWithValue("@Description", $"Full backup - Size: {fileInfo.Length / 1024.0 / 1024.0:F2} MB");
                        cmd.ExecuteNonQuery();
                    }
                }

                TempData["Success"] = "Sao lưu dữ liệu thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi sao lưu: " + ex.Message;
                System.Diagnostics.Debug.WriteLine($"Backup Error: {ex}");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Restore(string fileName)
        {
            try
            {
                string backupPath = Server.MapPath("~/MyDatabase/Backups");
                string filePath = Path.Combine(backupPath, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    throw new FileNotFoundException("File backup không tồn tại.");
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Kill active connections
                    string killQuery = @"
                    USE [master];
                    ALTER DATABASE WebThoiTrang SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
                    using (SqlCommand command = new SqlCommand(killQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // Restore database
                    string restoreQuery = $@"
                    RESTORE DATABASE [WebThoiTrang] 
                    FROM DISK = N'{filePath}'
                    WITH FILE = 1,
                    MOVE N'WebThoiTrang' TO N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\WebThoiTrang.mdf',
                    MOVE N'WebThoiTrang_log' TO N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\WebThoiTrang_log.ldf',
                    NOUNLOAD, REPLACE, STATS = 5;
                    
                    ALTER DATABASE WebThoiTrang SET MULTI_USER;";

                    using (SqlCommand command = new SqlCommand(restoreQuery, connection))
                    {
                        command.CommandTimeout = 300;
                        command.ExecuteNonQuery();
                    }
                }

                TempData["Success"] = "Phục hồi dữ liệu thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi phục hồi: " + ex.Message;
                System.Diagnostics.Debug.WriteLine($"Restore Error: {ex}");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult DeleteBackup(string fileName)
        {
            try
            {
                string backupPath = Server.MapPath("~/MyDatabase/Backups");
                string filePath = Path.Combine(backupPath, fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);

                    // Xóa record trong Backup_History
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string deleteQuery = "DELETE FROM [dbo].[Backup_History] WHERE [BackupName] = @Name";
                        using (SqlCommand cmd = new SqlCommand(deleteQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@Name", fileName);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    TempData["Success"] = "Đã xóa file backup thành công!";
                }
                else
                {
                    TempData["Error"] = "File backup không tồn tại!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi xóa file: " + ex.Message;
                System.Diagnostics.Debug.WriteLine($"Delete Error: {ex}");
            }

            return RedirectToAction("Index");
        }

        // Thêm hàm để lấy kích thước file theo định dạng
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }

        // Thêm hàm kiểm tra quyền Admin
        private bool IsAdmin()
        {
            var user = Session["TaiKhoan"] as KhachHang;
            return user != null && user.RoleID == 1;
        }
        private void Backup_PercentComplete(object sender, PercentCompleteEventArgs e)
        {
            // Cập nhật tiến trình backup
            var percentage = e.Percent;
            System.Diagnostics.Debug.WriteLine($"Backup Progress: {percentage}%");

            // Thông báo tiến trình qua TempData 
            TempData["Progress"] = percentage;
        }

        private void Backup_Complete(object sender, ServerMessageEventArgs e)
        {
            // Xử lý khi backup hoàn tất
            System.Diagnostics.Debug.WriteLine("Backup completed successfully!");

            // Lưu thông tin vào bảng Backup_History
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(@"
                        UPDATE [Backup_History] 
                        SET Description = Description + ' - Completed successfully' 
                        WHERE BackupID = (SELECT MAX(BackupID) FROM [Backup_History])", connection);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating backup history: {ex.Message}");
            }
        }

        private void Restore_PercentComplete(object sender, PercentCompleteEventArgs e)
        {
            // Cập nhật tiến trình restore
            var percentage = e.Percent;
            System.Diagnostics.Debug.WriteLine($"Restore Progress: {percentage}%");

            // Thông báo tiến trình
            TempData["Progress"] = percentage;
        }

        private void Restore_Complete(object sender, ServerMessageEventArgs e)
        {
            // Xử lý khi restore hoàn tất
            System.Diagnostics.Debug.WriteLine("Restore completed successfully!");

            // Ghi log vào bảng Backup_History
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(@"
                        INSERT INTO [Backup_History] (BackupName, Description)
                        VALUES ('Database Restore', 'Database restored successfully')", connection);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging restore completion: {ex.Message}");
            }
        }
    }
}