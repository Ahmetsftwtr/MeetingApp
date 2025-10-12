using MeetingApp.Api.Filters;
using MeetingApp.Business.Abstractions.Meeting;
using MeetingApp.Models.DTOs.Meeting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MeetingApp.Api.Controllers
{
    [JwtAuthorize]
    public class MeetingsController : BaseApiController
    {
        private readonly IMeetingService _meetingService;

        public MeetingsController(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Kullanýcý kimliði bulunamadý");
            }
            return userId;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMeeting([FromBody] CreateMeetingDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _meetingService.CreateMeetingAsync(userId, dto);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMeeting(Guid id, [FromBody] UpdateMeetingDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _meetingService.UpdateMeetingAsync(id, userId, dto);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeeting(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _meetingService.DeleteMeetingAsync(id, userId);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetMeetingById(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _meetingService.GetMeetingByIdAsync(id, userId);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllMeetings()
        {
            var userId = GetCurrentUserId();
            var result = await _meetingService.GetAllMeetingsAsync(userId);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingMeetings()
        {
            var userId = GetCurrentUserId();
            var result = await _meetingService.GetUpcomingMeetingsAsync(userId);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("past")]
        public async Task<IActionResult> GetPastMeetings()
        {
            var userId = GetCurrentUserId();
            var result = await _meetingService.GetPastMeetingsAsync(userId);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelMeeting(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _meetingService.CancelMeetingAsync(id, userId);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("cancelled")]
        public async Task<IActionResult> GetCancelledMeetings()
        {
            var userId = GetCurrentUserId();
            var result = await _meetingService.GetCancelledMeetingsAsync(userId);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
    }
}