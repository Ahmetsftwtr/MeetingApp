using MeetingApp.DataAccess.Repositories.Interfaces;
using MeetingApp.Model.Entities;
using MeetingApp.Models.DTOs.Meeting;
using System;
using System.Collections.Generic;
using System.Text;

namespace MeetingApp.DataAccess.Repositories.Abstractions
{
    public interface IMeetingRepository : IBaseRepository<Meeting>
    {
        Task<Meeting?> GetByIdWithDetailsAsync(Guid id);
        Task<(IEnumerable<Meeting> Meetings, int TotalCount)> GetFilteredMeetingsAsync(
            Guid userId,
            MeetingFilterDto filter);
        IQueryable<Meeting> GetAllQueryable();
    }
}
