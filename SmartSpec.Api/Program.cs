using Microsoft.EntityFrameworkCore;
using SmartSpec.Infrastructure;
using SmartSpec.Core.Interfaces;
using SmartSpec.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()  // 開發階段允許所有來源 (上線時建議改成指定網址)
              .AllowAnyMethod()  // 允許 GET, POST, PUT, DELETE
              .AllowAnyHeader(); // 允許任何標頭
    });
});

// 1. 設定資料庫連線
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SmartSpecDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. 註冊我們的文件服務 (IDocumentService)
builder.Services.AddScoped<IDocumentService, DocumentService>();

// 3. 【CORS 設定】允許跨域存取 (這是我們剛剛要加的)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. 【Swagger 設定】這段如果不見了，網頁就會 404
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll"); // <--- 關鍵！這行就是貼上通行證
app.UseStaticFiles(); // <--- 這行就像是把 wwwroot 的門打開，允許外界讀取檔案

// 5. 【啟用 CORS】(放在 UseHttpsRedirection 之後)
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();