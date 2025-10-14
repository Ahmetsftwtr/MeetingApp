using MeetingApp.Business.Abstractions.File;
using MeetingApp.DataAccess.Repositories.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingApp.Business.Services.Jobs
{
    public class MeetingCleanupJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly ILogger<MeetingCleanupJob> _logger;

        public MeetingCleanupJob(
            IUnitOfWork unitOfWork,
            IFileService fileService,
            ILogger<MeetingCleanupJob> logger)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task CleanupCancelledMeetingsAsync()
        {
            try
            {
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-7);

                _logger.LogInformation("Starting cleanup job for cancelled meetings older than {Date}", thirtyDaysAgo);

                var cancelledMeetings = await _unitOfWork.Meetings
                    .GetAllQueryable()
                    .Include(m => m.Documents)
                    .Where(m => m.IsCancelled && m.CancelledAt < thirtyDaysAgo)
                    .ToListAsync();

                if (!cancelledMeetings.Any())
                {
                    _logger.LogInformation("No cancelled meetings found for cleanup.");
                    return;
                }

                _logger.LogInformation("Found {Count} cancelled meetings to cleanup", cancelledMeetings.Count);

                int totalDeletedFiles = 0;
                int totalFailedFiles = 0;
                int totalDeletedMeetings = 0;

                foreach (var meeting in cancelledMeetings)
                {
                    try
                    {
                        _logger.LogInformation("Cleaning up meeting: {MeetingId} - {Title} (Cancelled at: {CancelledAt})",
                            meeting.Id,
                            meeting.Title,
                            meeting.CancelledAt);

                        var documentsToDelete = meeting.Documents.ToList();

                        foreach (var document in documentsToDelete)
                        {
                            try
                            {
                                var deleted = await _fileService.DeleteFileAsync(document.FilePath);
                                if (deleted)
                                {
                                    totalDeletedFiles++;
                                    _logger.LogDebug("Document file deleted: {FileName} from meeting {MeetingId}",
                                        document.FileName,
                                        meeting.Id);
                                }
                                else
                                {
                                    totalFailedFiles++;
                                    _logger.LogWarning("Document file not found: {FileName} ({FilePath}) from meeting {MeetingId}",
                                        document.FileName,
                                        document.FilePath,
                                        meeting.Id);
                                }
                            }
                            catch (Exception ex)
                            {
                                totalFailedFiles++;
                                _logger.LogError(ex, "Error deleting document file: {FileName} from meeting {MeetingId}",
                                    document.FileName,
                                    meeting.Id);
                            }
                        }

                        if (documentsToDelete.Any())
                        {
                            _unitOfWork.MeetingDocuments.RemoveRange(documentsToDelete);
                        }

                        _unitOfWork.Meetings.Remove(meeting);
                        totalDeletedMeetings++;

                        _logger.LogInformation(
                            "Meeting {MeetingId} prepared for deletion with {DocumentCount} documents",
                            meeting.Id,
                            documentsToDelete.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error preparing meeting {MeetingId} for deletion", meeting.Id);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Cleanup job completed successfully. Meetings deleted: {MeetingCount}, Files deleted: {FileCount}, Files failed: {FailedCount}",
                    totalDeletedMeetings,
                    totalDeletedFiles,
                    totalFailedFiles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error in cleanup job");
                throw;
            }
        }
    }
}