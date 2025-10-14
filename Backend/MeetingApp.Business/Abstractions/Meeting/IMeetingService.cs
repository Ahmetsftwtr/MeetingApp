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
        Task<IResult> GetMeetingByIdAsync(Guid meetingId, Guid userId);
        Task<IResult> GetFilteredMeetingsAsync(Guid userId, MeetingFilterDto filter);
        Task<IResult> CancelMeetingAsync(Guid meetingId, Guid userId);

    }
}