using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SmartSpec.Infrastructure;
using SmartSpec.Core.Interfaces;
using SmartSpec.Infrastructure.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. 設定資料庫連線
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SmartSpecDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. 註冊我們的文件服務 (IDocumentService)
builder.Services.AddScoped<IDocumentService, DocumentService>();

// 3. CORS 設定 (整理後保留這一份即可)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 4. 【新增】JWT 身分驗證設定
// 注意：這裡會讀取 appsettings.json 裡的 Jwt:Key
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // 確保 Key 足夠長，避免錯誤，這裡直接從設定檔讀取，若讀不到給預設值防呆
        var keyStr = builder.Configuration["Jwt:Key"] ?? "123789654asdjkl_SmartSpec_SecureKey_2026";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr))
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // 定義安全機制 (告訴 Swagger 我們是用 Bearer Token)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "請在下方輸入：Bearer+空格+你的Token"
    });

    // 套用安全需求 (讓所有 API 都受到保護)
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});



var app = builder.Build();

// Swagger 設定
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll"); // 開放跨域

app.UseStaticFiles();    // 開放靜態檔案 (wwwroot)

// 5. 【新增】啟用身分驗證 (檢查 Token)
// 重要：這行必須放在 UseCors 之後，UseAuthorization 之前
app.UseAuthentication();

app.UseAuthorization();  // 檢查權限

app.MapControllers();

app.Run();