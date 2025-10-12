using MeetingApp.DataAccess.Context;
using MeetingApp.DataAccess.Repositories.Abstractions;
using MeetingApp.DataAccess.Repositories.Base;
using MeetingApp.Model.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingApp.DataAccess.Repositories.Implementations
{
    public class MeetingRepository : BaseRepository<Meeting>, IMeetingRepository
    {
        public MeetingRepository(MeetingAppDbContext context) : base(context) { }

        public async Task<Meeting?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(m => m.User)
                .Include(m => m.Documents)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Meeting>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(m => m.User)
                .Include(m => m.Documents)
                .OrderByDescending(m => m.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Meeting>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(m => m.Documents)
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Meeting>> GetUpcomingMeetingsAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Include(m => m.Documents)
                .Where(m => m.UserId == userId && m.StartDate >= now)
                .OrderBy(m => m.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Meeting>> GetPastMeetingsAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Include(m => m.Documents)
                .Where(m => m.UserId == userId && m.EndDate < now)
                .OrderByDescending(m => m.StartDate)
                .ToListAsync();
        }

        public IQueryable<Meeting> GetAllQueryable()
        {
            return _dbSet.AsQueryable();
        }
    }
}