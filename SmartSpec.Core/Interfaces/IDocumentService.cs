using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SmartSpec.Core; // 引用 Document 類別

namespace SmartSpec.Core.Interfaces
{
    public interface IDocumentService
    {
        // 搜尋用
        Task<IEnumerable<Document>> SearchDocumentsAsync(string? keyword);

        // ✅ 上傳用 (我們要實作這個)
        // 參數：標題、檔案串流(內容)、原始檔名
        Task<Document> UploadDocumentAsync(string title, Stream fileStream, string originalFileName);

        // 下載用
        Task<(byte[] FileBytes, string FileName)?> DownloadDocumentAsync(Guid id);
    }
}