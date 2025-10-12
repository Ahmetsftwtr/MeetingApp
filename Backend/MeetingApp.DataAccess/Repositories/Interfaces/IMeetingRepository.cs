using MeetingApp.DataAccess.Repositories.Interfaces;
using MeetingApp.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MeetingApp.DataAccess.Repositories.Abstractions
{
    public interface IMeetingRepository : IBaseRepository<Meeting>
    {
        Task<Meeting?> GetByIdWithDetailsAsync(Guid id);
        Task<IEnumerable<Meeting>> GetAllWithDetailsAsync();
        Task<IEnumerable<Meeting>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Meeting>> GetUpcomingMeetingsAsync(Guid userId);
        Task<IEnumerable<Meeting>> GetPastMeetingsAsync(Guid userId);
        IQueryable<Meeting> GetAllQueryable();
    }
}
