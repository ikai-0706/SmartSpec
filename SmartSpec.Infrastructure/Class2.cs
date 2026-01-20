using Microsoft.EntityFrameworkCore;
using SmartSpec.Core;

namespace SmartSpec.Infrastructure
{
    public class SmartSpecDbContext : DbContext
    {
        // 固定寫法，把設定傳給父類別
        public SmartSpecDbContext(DbContextOptions<SmartSpecDbContext> options) : base(options)
        {
        }

        // 這行會告訴資料庫建立一個 "Documents" 的資料表
        public DbSet<Document> Documents { get; set; }
    }
}