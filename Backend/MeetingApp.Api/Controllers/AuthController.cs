using MeetingApp.Api.Filters;
using MeetingApp.Business.Abstractions.User;
using MeetingApp.Models.DTOs.File;
using MeetingApp.Models.DTOs.User;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MeetingApp.Api.Controllers
{
    public class AuthController : BaseApiController
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterDto dto, IFormFile? profileImage)
        {
            FileUploadDto? profileImageDto = null;

            if (profileImage != null && profileImage.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await profileImage.CopyToAsync(memoryStream);

                profileImageDto = new FileUploadDto
                {
                    OriginalFileName = profileImage.FileName,
                    ContentType = profileImage.ContentType,
                    FileSize = profileImage.Length,
                    FileBytes = memoryStream.ToArray()
                };
            }

            var result = await _userService.Register(dto, profileImageDto);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _userService.Login(dto);
            if (!result.IsSuccess)
                return Unauthorized(new { message = result.Message });

            return Ok(result);
        }

        [HttpGet("me")]
        [JwtAuthorize]
        public async Task<IActionResult> GetMyProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { success = false, message = "Kullanýcý kimliði bulunamadý" });

            var result = await _userService.GetProfile(userId);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
    }
}