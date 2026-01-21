using SmartSpec.Core;
using Xunit; // 引用測試框架

namespace SmartSpec.Tests
{
    public class DocumentTests
    {
        [Fact] // 這標籤代表這是一個測試案例
        public void NewDocument_Should_Have_Id_And_Date()
        {
            // Arrange (安排): 準備要測的物件
            // 我們想測試：當我 new 一個文件時...

            // Act (執行): 執行動作
            var doc = new Document();

            // Assert (斷言): 驗證結果是否符合預期
            // 1. ID 不應該是空的 (Guid.Empty)
            Assert.NotEqual(Guid.Empty, doc.Id);

            // 2. 上傳時間應該是最近的時間 (例如不早於 1 分鐘前)
            Assert.True(doc.UploadedAt > DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public void Document_Title_Should_Be_Set_Correctly()
        {
            // Arrange
            var title = "測試規格書";

            // Act
            var doc = new Document { Title = title };

            // Assert
            Assert.Equal("測試規格書", doc.Title);
        }
    }
}