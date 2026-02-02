using Microsoft.AspNetCore.Mvc;
using SmartSpec.Core.Interfaces;
using SmartSpec.Core;
using SmartSpec.Api.Dtos;   
using Microsoft.AspNetCore.Authorization; // <--- 記得加這行

namespace SmartSpec.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        // ==========================================
        // 👇 這裡改回來了：專門負責「取得全部」
        // ==========================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> GetDocuments()
        {
            // 1. 從 Service 拿到原始資料 (Entity)
            var documents = await _documentService.SearchDocumentsAsync(null);

            // 2. 轉換成 DTO (Entity -> DTO)
            var dtos = documents.Select(d => new DocumentDto
            {
                Id = d.Id,
                Title = d.Title,
                UploadedAt = d.UploadedAt,
                // 我們可以在這裡做一點邏輯，例如只回傳副檔名
                FileExtension = Path.GetExtension(d.FilePath)
            });

            return Ok(dtos);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> SearchDocuments([FromQuery] string keyword)
        {
            var documents = await _documentService.SearchDocumentsAsync(keyword);

            // 同樣做轉換
            var dtos = documents.Select(d => new DocumentDto
            {
                Id = d.Id,
                Title = d.Title,
                UploadedAt = d.UploadedAt,
                FileExtension = Path.GetExtension(d.FilePath)
            });

            return Ok(dtos);
        }

        // [Security] 加上 Authorize 標籤，代表此 API 需要 JWT Token 才能存取
        [Authorize]

        [HttpPost("upload")]
        public async Task<IActionResult> UploadDocument(IFormFile file, [FromForm] string title)
        {
            if (file == null || file.Length == 0) return BadRequest("請選擇檔案");

            using var stream = file.OpenReadStream();
            var document = await _documentService.UploadDocumentAsync(title, stream, file.FileName);

            return Ok(new { Message = "上傳成功", DocId = document.Id });
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadDocument(Guid id)
        {
            var result = await _documentService.DownloadDocumentAsync(id);

            if (result == null) return NotFound("找不到檔案");

            return File(result.Value.FileBytes, "application/octet-stream", result.Value.FileName);
        }
    }
}