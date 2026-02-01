using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartSpec.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // 登入 API: 接收帳密，回傳 Token
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // 這裡模擬驗證：帳號 admin, 密碼 password
            // (面試時說明：真實專案會連線資料庫 User 表，這裡為 Demo 方便先寫死)
            if (request.Username == "admin" && request.Password == "password")
            {
                var token = GenerateJwtToken(request.Username);
                return Ok(new { Token = token });
            }

            return Unauthorized("帳號或密碼錯誤");
        }

        // 產生 JWT Token 的核心邏輯
        private string GenerateJwtToken(string username)
        {
            // 1. 從設定檔讀取密鑰 (移除原本的 ?? "..." 預設值)
            var keyStr = _configuration["Jwt:Key"];

            // [資安修正] Fail Fast: 如果沒設定金鑰，直接報錯，不要偷偷用預設值
            if (string.IsNullOrEmpty(keyStr))
            {
                throw new InvalidOperationException("嚴重錯誤: appsettings.json 中缺少 Jwt:Key 設定，系統無法啟動。");
            }

            // [資安修正] 檢查長度: HMACSHA256 至少需要 32 bytes (256 bits)
            if (keyStr.Length < 32)
            {
                throw new InvalidOperationException("嚴重錯誤: Jwt:Key 長度不足，至少需要 32 個字元以確保安全。");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 2. 設定 Token 內容 (Claims)
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username), // 使用者名稱
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Token 唯一 ID
            };

            // 3. 建立 Token 物件
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1), // 有效期 1 小時
                signingCredentials: creds
            );

            // 4. 寫出 Token 字串
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // 定義登入用的資料結構
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}