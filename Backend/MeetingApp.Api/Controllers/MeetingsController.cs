using MeetingApp.Api.Filters;
using MeetingApp.Business.Abstractions.Meeting;
using MeetingApp.Models.DTOs.Meeting;
using Microsoft.AspNetCore.Mvc;
using System;
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

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateMeetingDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _meetingService.CreateMeetingAsync(userId, dto);
            return HandleResult(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMeetingDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _meetingService.UpdateMeetingAsync(id, userId, dto);
            return HandleResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _meetingService.DeleteMeetingAsync(id, userId);
            return HandleResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _meetingService.GetMeetingByIdAsync(id, userId);
            return HandleResult(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] MeetingFilterDto filter)
        {
            var userId = GetCurrentUserId();
            var result = await _meetingService.GetFilteredMeetingsAsync(userId, filter);
            return HandleResult(result);
        }

        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _meetingService.CancelMeetingAsync(id, userId);
            return HandleResult(result);
        }
    }
}