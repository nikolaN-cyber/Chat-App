using Core.Services;
using Core.Types;
using Microsoft.AspNetCore.Mvc;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly FileService _fileService;

        public FileController(FileService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost("upload-chat-file")]
        public async Task<IActionResult> UploadChatFile(IFormFile file, CancellationToken ct)
        {
            try
            {
                var relativePath = await _fileService.SaveChatFileAsync(file, ct);
                return Ok(ApiResponse<object>.SuccessResponse(new { url = relativePath }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
            }
        }
    }
}
