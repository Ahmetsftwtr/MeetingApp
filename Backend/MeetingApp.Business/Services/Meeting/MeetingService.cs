using MeetingApp.Business.Abstractions.Email;
using MeetingApp.Business.Abstractions.Meeting;
using MeetingApp.Business.Mappings;
using MeetingApp.DataAccess.Repositories.UnitOfWork;
using MeetingApp.Models.DTOs.Common;
using MeetingApp.Models.DTOs.Meeting;
using MeetingApp.Models.ReturnTypes.Abstract;
using MeetingApp.Models.ReturnTypes.Concrete;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingApp.Business.Services.Meeting
{
    public class MeetingService : IMeetingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogger<MeetingService> _logger;
        private readonly string _baseUrl;

        public MeetingService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IEmailService emailService,
            ILogger<MeetingService> logger)
        {
            _unitOfWork = unitOfWork;
            _baseUrl = configuration["AppSettings:BaseUrl"] ?? "https://localhost:5001";
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<IResult> CreateMeetingAsync(Guid userId, CreateMeetingDto dto)
        {
            try
            {
                _logger.LogInformation("Meeting creation started by user: {UserId}", userId);

                if (dto.EndDate <= dto.StartDate)
                {
                    _logger.LogWarning("Meeting creation failed: Invalid date range for user {UserId}", userId);
                    return new ErrorResult("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
                }

                if (dto.StartDate < DateTime.UtcNow)
                {
                    _logger.LogWarning("Meeting creation failed: Past date selected by user {UserId}", userId);
                    return new ErrorResult("Geçmiş tarihli toplantı oluşturamazsınız.");
                }

                var meeting = MeetingMapper.ToEntity(dto, userId);

                await _unitOfWork.Meetings.AddAsync(meeting);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Meeting created successfully: {MeetingId} - {Title} by user {UserId}", meeting.Id, meeting.Title, userId);

                var createdMeeting = await _unitOfWork.Meetings.GetByIdWithDetailsAsync(meeting.Id);
                var resultDto = MeetingMapper.ToDto(createdMeeting!, _baseUrl);

                var user = await _unitOfWork.Users.GetUserByIdAsync(userId);

                _emailService.QueueMeetingCreatedEmail(
                    user.Email,
                    $"{user.FirstName} {user.LastName}",
                    meeting.Id,
                    meeting.Title,
                    meeting.StartDate,
                    meeting.EndDate,
                    meeting.Description ?? string.Empty
                );

                _logger.LogInformation("Meeting creation email queued for meeting: {MeetingId}", meeting.Id);

                return new SuccessDataResult<MeetingDto>(resultDto, "Toplantı başarıyla oluşturuldu.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error during meeting creation by user: {UserId}", userId);
                return new ErrorResult($"Veritabanı hatası: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during meeting creation by user: {UserId}", userId);
                return new ErrorResult($"Toplantı oluşturulurken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> UpdateMeetingAsync(Guid meetingId, Guid userId, UpdateMeetingDto dto)
        {
            try
            {
                _logger.LogInformation("Meeting update started: {MeetingId} by user: {UserId}", meetingId, userId);

                var meeting = await _unitOfWork.Meetings.GetByIdWithDetailsAsync(meetingId);
                if (meeting == null)
                {
                    _logger.LogWarning("Meeting update failed: Meeting not found - {MeetingId}", meetingId);
                    return new ErrorResult("Toplantı bulunamadı.");
                }

                if (meeting.UserId != userId)
                {
                    _logger.LogWarning("Meeting update failed: Unauthorized access by user {UserId} to meeting {MeetingId}", userId, meetingId);
                    return new ErrorResult("Bu toplantıyı güncelleme yetkiniz yok.");
                }

                if (dto.EndDate <= dto.StartDate)
                {
                    _logger.LogWarning("Meeting update failed: Invalid date range for meeting {MeetingId}", meetingId);
                    return new ErrorResult("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
                }

                if (meeting.EndDate < DateTime.UtcNow)
                {
                    _logger.LogWarning("Meeting update failed: Cannot update past meeting {MeetingId}", meetingId);
                    return new ErrorResult("Geçmiş toplantılar güncellenemez.");
                }

                MeetingMapper.UpdateEntity(meeting, dto);

                _unitOfWork.Meetings.Update(meeting);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Meeting updated successfully: {MeetingId} - {Title}", meetingId, meeting.Title);

                var updatedMeeting = await _unitOfWork.Meetings.GetByIdWithDetailsAsync(meetingId);
                var resultDto = MeetingMapper.ToDto(updatedMeeting!, _baseUrl);

                return new SuccessDataResult<MeetingDto>(resultDto, "Toplantı başarıyla güncellendi.");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error during meeting update: {MeetingId}", meetingId);
                return new ErrorResult("Toplantı başka bir kullanıcı tarafından değiştirildi. Lütfen sayfayı yenileyin.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error during meeting update: {MeetingId}", meetingId);
                return new ErrorResult($"Veritabanı hatası: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during meeting update: {MeetingId}", meetingId);
                return new ErrorResult($"Toplantı güncellenirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> DeleteMeetingAsync(Guid meetingId, Guid userId)
        {
            try
            {
                _logger.LogInformation("Meeting deletion started: {MeetingId} by user: {UserId}", meetingId, userId);

                var meeting = await _unitOfWork.Meetings.GetByIdWithDetailsAsync(meetingId);
                if (meeting == null)
                {
                    _logger.LogWarning("Meeting deletion failed: Meeting not found - {MeetingId}", meetingId);
                    return new ErrorResult("Toplantı bulunamadı.");
                }

                if (meeting.UserId != userId)
                {
                    _logger.LogWarning("Meeting deletion failed: Unauthorized access by user {UserId} to meeting {MeetingId}", userId, meetingId);
                    return new ErrorResult("Bu toplantıyı silme yetkiniz yok.");
                }

                if (meeting.EndDate < DateTime.UtcNow)
                {
                    _logger.LogWarning("Meeting deletion failed: Cannot delete past meeting {MeetingId}", meetingId);
                    return new ErrorResult("Geçmiş toplantılar silinemez.");
                }

                _unitOfWork.Meetings.Remove(meeting);
                _unitOfWork.MeetingDocuments.RemoveRange(meeting.Documents);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Meeting deleted successfully: {MeetingId} - {Title} (Trigger will log to DeletedMeetingsLog)", meetingId, meeting.Title);

                return new SuccessResult("Toplantı başarıyla silindi.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error during meeting deletion: {MeetingId}", meetingId);
                return new ErrorResult($"Veritabanı hatası: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during meeting deletion: {MeetingId}", meetingId);
                return new ErrorResult($"Toplantı silinirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> GetMeetingByIdAsync(Guid meetingId, Guid userId)
        {
            try
            {
                _logger.LogInformation("Fetching meeting: {MeetingId} by user: {UserId}", meetingId, userId);

                var meeting = await _unitOfWork.Meetings.GetByIdWithDetailsAsync(meetingId);

                if (meeting == null)
                {
                    _logger.LogWarning("Meeting not found: {MeetingId}", meetingId);
                    return new ErrorResult("Toplantı bulunamadı.");
                }

                if (meeting.UserId != userId)
                {
                    _logger.LogWarning("Unauthorized access attempt by user {UserId} to meeting {MeetingId}", userId, meetingId);
                    return new ErrorResult("Bu toplantıyı görüntüleme yetkiniz yok.");
                }

                var resultDto = MeetingMapper.ToDto(meeting, _baseUrl);
                return new SuccessDataResult<MeetingDto>(resultDto, "Toplantı getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching meeting: {MeetingId}", meetingId);
                return new ErrorResult($"Toplantı getirilirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> GetFilteredMeetingsAsync(Guid userId, MeetingFilterDto filter)
        {
            try
            {
                _logger.LogInformation(
                    "Fetching filtered meetings for user: {UserId} with status: {Status}, page: {PageNumber}, pageSize: {PageSize}",
                    userId,
                    filter.Status,
                    filter.PageNumber,
                    filter.PageSize);

                if (filter.PageNumber < 1)
                    filter.PageNumber = 1;

                if (filter.PageSize < 1 || filter.PageSize > 100)
                    filter.PageSize = 10;

                var (meetings, totalCount) = await _unitOfWork.Meetings.GetFilteredMeetingsAsync(userId, filter);

                var meetingDtos = MeetingMapper.ToDtoList(meetings, _baseUrl).ToList();
                var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

                var pagedResult = new PagedResultDto<MeetingDto>
                {
                    Items = meetingDtos,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                _logger.LogInformation(
                    "Found {Count} meetings (page {PageNumber} of {TotalPages}) for user: {UserId}",
                    meetingDtos.Count,
                    filter.PageNumber,
                    totalPages,
                    userId);

                return new SuccessDataResult<PagedResultDto<MeetingDto>>(
                    pagedResult,
                    $"Sayfa {filter.PageNumber}/{totalPages} - Toplam {totalCount} toplantıdan {meetingDtos.Count} tanesi getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching filtered meetings for user: {UserId}", userId);
                return new ErrorResult($"Toplantılar getirilirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> CancelMeetingAsync(Guid meetingId, Guid userId)
        {
            try
            {
                _logger.LogInformation("Meeting cancellation started: {MeetingId} by user: {UserId}", meetingId, userId);

                var meeting = await _unitOfWork.Meetings.GetByIdWithDetailsAsync(meetingId);
                if (meeting == null)
                {
                    _logger.LogWarning("Meeting cancellation failed: Meeting not found - {MeetingId}", meetingId);
                    return new ErrorResult("Toplantı bulunamadı.");
                }

                if (meeting.UserId != userId)
                {
                    _logger.LogWarning("Meeting cancellation failed: Unauthorized access by user {UserId} to meeting {MeetingId}", userId, meetingId);
                    return new ErrorResult("Bu toplantıyı iptal etme yetkiniz yok.");
                }

                if (meeting.IsCancelled)
                {
                    _logger.LogWarning("Meeting cancellation failed: Meeting already cancelled - {MeetingId}", meetingId);
                    return new ErrorResult("Toplantı zaten iptal edilmiş.");
                }

                if (meeting.EndDate < DateTime.UtcNow)
                {
                    _logger.LogWarning("Meeting cancellation failed: Cannot cancel past meeting {MeetingId}", meetingId);
                    return new ErrorResult("Geçmiş toplantılar iptal edilemez.");
                }

                meeting.IsCancelled = true;
                meeting.CancelledAt = DateTime.UtcNow;
                meeting.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Meetings.Update(meeting);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Meeting cancelled successfully: {MeetingId} - {Title} at {CancelledAt}", meetingId, meeting.Title, meeting.CancelledAt);

                return new SuccessResult("Toplantı başarıyla iptal edildi.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error during meeting cancellation: {MeetingId}", meetingId);
                return new ErrorResult($"Veritabanı hatası: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during meeting cancellation: {MeetingId}", meetingId);
                return new ErrorResult($"Toplantı iptal edilirken bir hata oluştu: {ex.Message}");
            }
        }
    }
}