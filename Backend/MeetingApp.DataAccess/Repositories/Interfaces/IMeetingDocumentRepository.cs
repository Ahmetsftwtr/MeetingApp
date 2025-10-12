using MeetingApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MeetingApp.DataAccess.Repositories.Interfaces
{
    public interface IMeetingDocumentRepository : IBaseRepository<MeetingDocument>
    {
        Task<IEnumerable<MeetingDocument>> GetByMeetingIdAsync(Guid meetingId);
        Task<MeetingDocument?> GetByIdAsync(Guid id);
    }
}
