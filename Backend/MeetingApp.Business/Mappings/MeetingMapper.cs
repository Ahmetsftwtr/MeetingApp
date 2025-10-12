using MeetingApp.Model.Entities;
using MeetingApp.Models.DTOs.Meeting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MeetingApp.Business.Mappings
{
    public static class MeetingMapper
    {
        public static MeetingDto ToDto(Meeting meeting, string baseUrl)
        {
            return new MeetingDto
            {
                Id = meeting.Id,
                Title = meeting.Title,
                Description = meeting.Description,
                StartDate = meeting.StartDate,
                EndDate = meeting.EndDate,
                CreatedAt = meeting.CreatedAt,
                UpdatedAt = meeting.UpdatedAt,
                IsCancelled = meeting.IsCancelled,
                CancelledAt = meeting.CancelledAt,
                UserId = meeting.UserId,
                Documents = meeting.Documents?.Select(d => MeetingDocumentMapper.ToDto(d, baseUrl)).ToList()
                    ?? new List<MeetingDocumentDto>()
            };
        }

        public static Meeting ToEntity(CreateMeetingDto dto, Guid userId)
        {
            return new Meeting
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static void UpdateEntity(Meeting meeting, UpdateMeetingDto dto)
        {
            meeting.Title = dto.Title;
            meeting.Description = dto.Description;
            meeting.StartDate = dto.StartDate;
            meeting.EndDate = dto.EndDate;
            meeting.UpdatedAt = DateTime.UtcNow;
        }

        public static IEnumerable<MeetingDto> ToDtoList(IEnumerable<Meeting> meetings, string baseUrl)
        {
            return meetings.Select(m => ToDto(m, baseUrl)).ToList();
        }
    }
}