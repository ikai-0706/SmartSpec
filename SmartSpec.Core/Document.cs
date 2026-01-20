using System;

namespace SmartSpec.Core
{
    public class Document
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; }

        // ✅ 關鍵在這裡！
        // 這個「建構子」保證每次建立新文件時，都會給一個全新的 ID
        public Document()
        {
            Id = Guid.NewGuid();          // 自動產生亂數 ID
            UploadedAt = DateTime.UtcNow; // 自動填入現在時間
        }
    }
}