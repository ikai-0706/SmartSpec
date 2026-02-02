namespace SmartSpec.Api.Dtos
{
    // 這就是我們要給前端看的樣子，乾淨、只有必要的資訊
    public class DocumentDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime UploadedAt { get; set; }
        // 注意：我們故意不放 FilePath，保護伺服器路徑安全
        public string FileExtension { get; set; } // 可以額外加一個副檔名給前端顯示圖示用
    }
}