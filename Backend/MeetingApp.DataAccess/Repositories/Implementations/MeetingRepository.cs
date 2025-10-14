using MeetingApp.DataAccess.Context;
using MeetingApp.DataAccess.Repositories.Abstractions;
using MeetingApp.DataAccess.Repositories.Base;
using MeetingApp.Model.Entities;
using MeetingApp.Models.DTOs.Meeting;
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

        public async Task<(IEnumerable<Meeting> Meetings, int TotalCount)> GetFilteredMeetingsAsync(
            Guid userId,
            MeetingFilterDto filter)
        {
            var query = _dbSet
                .Include(m => m.Documents)
                .Where(m => m.UserId == userId);

            var now = DateTime.UtcNow;
            query = filter.Status switch
            {
                MeetingStatus.Upcoming => query.Where(m => m.StartDate >= now && !m.IsCancelled),
                MeetingStatus.Past => query.Where(m => m.EndDate < now && !m.IsCancelled),
                MeetingStatus.Cancelled => query.Where(m => m.IsCancelled),
                MeetingStatus.Active => query.Where(m => m.StartDate >= now && !m.IsCancelled),
                _ => query
            };

            if (filter.StartDateFrom.HasValue)
                query = query.Where(m => m.StartDate >= filter.StartDateFrom.Value);

            if (filter.StartDateTo.HasValue)
                query = query.Where(m => m.StartDate <= filter.StartDateTo.Value);

            if (filter.EndDateFrom.HasValue)
                query = query.Where(m => m.EndDate >= filter.EndDateFrom.Value);

            if (filter.EndDateTo.HasValue)
                query = query.Where(m => m.EndDate <= filter.EndDateTo.Value);

            if (filter.IsCancelled.HasValue)
                query = query.Where(m => m.IsCancelled == filter.IsCancelled.Value);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(m =>
                    m.Title.ToLower().Contains(searchTerm) ||
                    (m.Description != null && m.Description.ToLower().Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();

            query = filter.OrderBy?.ToLower() switch
            {
                "title" => filter.IsDescending
                    ? query.OrderByDescending(m => m.Title)
                    : query.OrderBy(m => m.Title),
                "enddate" => filter.IsDescending
                    ? query.OrderByDescending(m => m.EndDate)
                    : query.OrderBy(m => m.EndDate),
                "createdat" => filter.IsDescending
                    ? query.OrderByDescending(m => m.CreatedAt)
                    : query.OrderBy(m => m.CreatedAt),
                _ => filter.IsDescending
                    ? query.OrderByDescending(m => m.StartDate)
                    : query.OrderBy(m => m.StartDate)
            };

            var meetings = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (meetings, totalCount);
        }

        public IQueryable<Meeting> GetAllQueryable()
        {
            return _dbSet.AsQueryable();
        }
    }
}