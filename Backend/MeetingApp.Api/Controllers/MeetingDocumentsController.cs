using MeetingApp.Api.Filters;
using MeetingApp.Business.Abstractions.Meeting;
using MeetingApp.Models.DTOs.Meeting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MeetingApp.Api.Controllers
{
    [JwtAuthorize]
    public class MeetingDocumentsController : BaseApiController
    {
        private readonly IMeetingDocumentService _documentService;

        public MeetingDocumentsController(IMeetingDocumentService documentService)
        {
            _documentService = documentService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("Kullanýcý kimliði bulunamadý");
            return userId;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(Guid meetingId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "Dosya seçilmedi." });

            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            var dto = new UploadDocumentDto
            {
                FileName = file.FileName,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                FileBytes = fileBytes
            };

            var userId = GetCurrentUserId();
            var result = await _documentService.UploadDocumentAsync(meetingId, userId, dto);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetList(Guid meetingId)
        {
            var userId = GetCurrentUserId();
            var result = await _documentService.GetDocumentsByMeetingIdAsync(meetingId, userId);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{documentId}/download")]
        public async Task<IActionResult> Download(Guid meetingId, Guid documentId)
        {
            var userId = GetCurrentUserId();
            var result = await _documentService.DownloadDocumentAsync(documentId, userId);

            if (!result.IsSuccess)
                return BadRequest(result);

            var data = (ValueTuple<byte[], string, string>)((dynamic)result).Data;
            return File(data.Item1, data.Item2, data.Item3);
        }

        [HttpDelete("{documentId}")]
        public async Task<IActionResult> Delete(Guid meetingId, Guid documentId)
        {
            var userId = GetCurrentUserId();
            var result = await _documentService.DeleteDocumentAsync(documentId, userId);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
    }
}