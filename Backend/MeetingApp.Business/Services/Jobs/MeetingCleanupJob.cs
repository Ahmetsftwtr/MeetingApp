using MeetingApp.DataAccess.Repositories.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingApp.Business.Services.Jobs
{
    public class MeetingCleanupJob
    {
        private readonly IUnitOfWork _unitOfWork;

        public MeetingCleanupJob(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CleanupCancelledMeetingsAsync()
        {
            try
            {
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-7);

                var cancelledMeetings = await _unitOfWork.Meetings
                    .GetAllQueryable()
                    .Where(m => m.IsCancelled && m.CancelledAt < thirtyDaysAgo)
                    .ToListAsync();

                if (cancelledMeetings.Any())
                {
                    foreach (var meeting in cancelledMeetings)
                    {
                        _unitOfWork.Meetings.Remove(meeting);
                    }

                    await _unitOfWork.SaveChangesAsync();
                    Console.WriteLine($"{cancelledMeetings.Count} iptal edilmiş toplantı temizlendi.");
                }
                else
                {
                    Console.WriteLine("Temizlenecek iptal edilmiş toplantı bulunamadı.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Toplantı temizleme hatası: {ex.Message}");
            }
        }
    }
}