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

        // 實作上傳
        public async Task<Document> UploadDocumentAsync(string title, Stream fileStream, string originalFileName)
        {
            // 1. 決定檔案要存去哪裡
            // 取得目前專案執行的目錄 (SmartSpec.Api)
            var currentDirectory = Directory.GetCurrentDirectory();
            var uploadsFolder = Path.Combine(currentDirectory, "wwwroot", "uploads");

            // 如果資料夾不存在，就建立它 (防呆)
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // 2. 產生一個安全的新檔名 (避免檔名重複或含有特殊字元)
            // 例如：原始 "規格書.pdf" -> 存成 "Guid-規格書.pdf"
            var fileExtension = Path.GetExtension(originalFileName);
            var safeFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, safeFileName);

            // 3. 將檔案寫入硬碟
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(stream);
            }

            // 4. 建立資料庫紀錄
            // 注意：FilePath 我們存 "相對路徑"，方便之後網頁讀取
            var document = new Document
            {
                Id = Guid.NewGuid(),
                Title = title,
                FilePath = $"uploads/{safeFileName}", // 存這個路徑
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

            // 檢查資料庫有沒有，且硬碟檔案還在不在
            if (document == null || !File.Exists(document.FilePath)) return null;

            var fileBytes = await File.ReadAllBytesAsync(document.FilePath);
            var fileName = Path.GetFileName(document.FilePath);

            return (fileBytes, fileName);
        }
    }
}