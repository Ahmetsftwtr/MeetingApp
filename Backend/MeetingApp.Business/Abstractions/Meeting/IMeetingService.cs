using MeetingApp.Models.DTOs.Meeting;
using MeetingApp.Models.ReturnTypes.Abstract;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeetingApp.Business.Abstractions.Meeting
{
    public interface IMeetingService
    {
            Task<IResult> CreateMeetingAsync(Guid userId, CreateMeetingDto dto);
            Task<IResult> UpdateMeetingAsync(Guid meetingId, Guid userId, UpdateMeetingDto dto);
            Task<IResult> DeleteMeetingAsync(Guid meetingId, Guid userId);
            Task<IResult> CancelMeetingAsync(Guid meetingId, Guid userId);
            Task<IResult> GetMeetingByIdAsync(Guid meetingId, Guid userId);
            Task<IResult> GetAllMeetingsAsync(Guid userId);
            Task<IResult> GetCancelledMeetingsAsync(Guid userId);
            Task<IResult> GetUpcomingMeetingsAsync(Guid userId);
            Task<IResult> GetPastMeetingsAsync(Guid userId);
        
    }
}