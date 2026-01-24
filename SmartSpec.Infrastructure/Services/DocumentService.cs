using Microsoft.EntityFrameworkCore;
using SmartSpec.Core;
using SmartSpec.Core.Interfaces;

namespace SmartSpec.Infrastructure.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly SmartSpecDbContext _context;

        public DocumentService(SmartSpecDbContext context)
        {
            _context = context;
        }

        // 實作搜尋
        public async Task<IEnumerable<Document>> SearchDocumentsAsync(string? keyword)
        {
            var query = _context.Documents.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(d => d.Title.Contains(keyword));
            }
            return await query.ToListAsync();
        }

        // 實作上傳 (已加入防呆檢查)
        public async Task<Document> UploadDocumentAsync(string title, Stream fileStream, string originalFileName)
        {
            // ==========================================
            // 👇 1. [新增] 檢查檔案大小 (限制 10 MB)
            // ==========================================
            const long maxFileSize = 10 * 1024 * 1024; // 10 MB
            if (fileStream.Length > maxFileSize)
            {
                throw new ArgumentException($"檔案過大！請上傳小於 {maxFileSize / 1024 / 1024} MB 的檔案。");
            }

            // ==========================================
            // 👇 2. [新增] 檢查副檔名 (白名單機制)
            // ==========================================
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
            var fileExtension = Path.GetExtension(originalFileName).ToLowerInvariant(); // 轉小寫比對

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new ArgumentException($"不支援的檔案格式：{fileExtension}。僅允許：PDF, 圖片, Word 文件。");
            }

            // ==========================================
            // 👇 以下是原本的儲存邏輯
            // ==========================================

            // 決定檔案要存去哪裡 (SmartSpec.Api/wwwroot/uploads)
            var currentDirectory = Directory.GetCurrentDirectory();
            var uploadsFolder = Path.Combine(currentDirectory, "wwwroot", "uploads");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // 產生亂數檔名
            var safeFileName = $"{Guid.NewGuid()}{fileExtension}";
            var fullPath = Path.Combine(uploadsFolder, safeFileName); // 這是存到硬碟的「絕對路徑」

            // 寫入硬碟
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await fileStream.CopyToAsync(stream);
            }

            // 建立資料庫紀錄
            var document = new Document
            {
                Id = Guid.NewGuid(),
                Title = title,
                // 資料庫存相對路徑，方便前端 <img> 或 <a> 標籤使用
                FilePath = $"uploads/{safeFileName}",
                UploadedAt = DateTime.UtcNow
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return document;
        }

        // 實作下載
        public async Task<(byte[] FileBytes, string FileName)?> DownloadDocumentAsync(Guid id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return null;

            // ⚠️ 修正路徑讀取：必須組出完整的硬碟路徑，不然 File.ReadAllBytesAsync 會找不到
            var currentDirectory = Directory.GetCurrentDirectory();
            // document.FilePath 是 "uploads/xxx.pdf"，所以要把它拼在 "wwwroot" 後面
            var fullPath = Path.Combine(currentDirectory, "wwwroot", document.FilePath);

            if (!File.Exists(fullPath)) return null;

            var fileBytes = await File.ReadAllBytesAsync(fullPath);
            var fileName = Path.GetFileName(fullPath);

            return (fileBytes, fileName);
        }
    }
}