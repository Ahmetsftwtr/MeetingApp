using MeetingApp.DataAccess.Context;
using MeetingApp.DataAccess.Repositories.Abstractions;
using MeetingApp.DataAccess.Repositories.Base;
using MeetingApp.DataAccess.Repositories.Interfaces;
using MeetingApp.Model.Entities;
using MeetingApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingApp.DataAccess.Repositories.Implementations
{
    public class MeetingDocumentRepository : BaseRepository<MeetingDocument>, IMeetingDocumentRepository
    {
        public MeetingDocumentRepository(MeetingAppDbContext context) : base(context) { }

        public async Task<IEnumerable<MeetingDocument>> GetByMeetingIdAsync(Guid meetingId)
        {
            return await _dbSet
                .Where(d => d.MeetingId == meetingId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
        }

        public async Task<MeetingDocument?> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(d => d.Meeting)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
    }
}