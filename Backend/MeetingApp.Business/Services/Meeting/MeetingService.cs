using MeetingApp.Business.Abstractions.Email;
using MeetingApp.Business.Abstractions.Meeting;
using MeetingApp.Business.Mappings;
using MeetingApp.DataAccess.Repositories.UnitOfWork;
using MeetingApp.Models.DTOs.Meeting;
using MeetingApp.Models.ReturnTypes.Abstract;
using MeetingApp.Models.ReturnTypes.Concrete;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingApp.Business.Services.Meeting
{
    public class MeetingService : IMeetingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly string _baseUrl;

        public MeetingService(IUnitOfWork unitOfWork, IConfiguration configuration, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _baseUrl = configuration["AppSettings:BaseUrl"] ?? "https://localhost:5001";
            _emailService = emailService;
        }

        public async Task<IResult> CreateMeetingAsync(Guid userId, CreateMeetingDto dto)
        {
            try
            {
                if (dto.EndDate <= dto.StartDate)
                {
                    return new ErrorResult("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
                }

                if (dto.StartDate < DateTime.UtcNow)
                {
                    return new ErrorResult("Geçmiş tarihli toplantı oluşturamazsınız.");
                }

                var meeting = MeetingMapper.ToEntity(dto, userId);

                await _unitOfWork.Meetings.AddAsync(meeting);
                await _unitOfWork.SaveChangesAsync();

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

                return new SuccessDataResult<MeetingDto>(resultDto, "Toplantı başarıyla oluşturuldu.");
            }
            catch (DbUpdateException ex)
            {
                return new ErrorResult($"Veritabanı hatası: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Toplantı oluşturulurken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> UpdateMeetingAsync(Guid meetingId, Guid userId, UpdateMeetingDto dto)
        {
            try
            {
                var meeting = await _unitOfWork.Meetings.GetByIdWithDetailsAsync(meetingId);
                if (meeting == null)
                {
                    return new ErrorResult("Toplantı bulunamadı.");
                }

                if (meeting.UserId != userId)
                {
                    return new ErrorResult("Bu toplantıyı güncelleme yetkiniz yok.");
                }

                if (dto.EndDate <= dto.StartDate)
                {
                    return new ErrorResult("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
                }

                if (meeting.EndDate < DateTime.UtcNow)
                {
                    return new ErrorResult("Geçmiş toplantılar güncellenemez.");
                }

                MeetingMapper.UpdateEntity(meeting, dto);

                _unitOfWork.Meetings.Update(meeting);
                await _unitOfWork.SaveChangesAsync();

                var updatedMeeting = await _unitOfWork.Meetings.GetByIdWithDetailsAsync(meetingId);
                var resultDto = MeetingMapper.ToDto(updatedMeeting!, _baseUrl);

                return new SuccessDataResult<MeetingDto>(resultDto, "Toplantı başarıyla güncellendi.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return new ErrorResult("Toplantı başka bir kullanıcı tarafından değiştirildi. Lütfen sayfayı yenileyin.");
            }
            catch (DbUpdateException ex)
            {
                return new ErrorResult($"Veritabanı hatası: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Toplantı güncellenirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> DeleteMeetingAsync(Guid meetingId, Guid userId)
        {
            try
            {
                var meeting = await _unitOfWork.Meetings.GetByIdWithDetailsAsync(meetingId);
                if (meeting == null)
                {
                    return new ErrorResult("Toplantı bulunamadı.");
                }

                if (meeting.UserId != userId)
                {
                    return new ErrorResult("Bu toplantıyı silme yetkiniz yok.");
                }

                if (meeting.EndDate < DateTime.UtcNow)
                {
                    return new ErrorResult("Geçmiş toplantılar silinemez.");
                }

                _unitOfWork.Meetings.Remove(meeting);
                await _unitOfWork.SaveChangesAsync();

                return new SuccessResult("Toplantı başarıyla silindi.");
            }
            catch (DbUpdateException ex)
            {
                return new ErrorResult($"Veritabanı hatası: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Toplantı silinirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> GetMeetingByIdAsync(Guid meetingId, Guid userId)
        {
            try
            {
                var meeting = await _unitOfWork.Meetings.GetByIdWithDetailsAsync(meetingId);

                if (meeting == null)
                {
                    return new ErrorResult("Toplantı bulunamadı.");
                }

                if (meeting.UserId != userId)
                {
                    return new ErrorResult("Bu toplantıyı görüntüleme yetkiniz yok.");
                }

                var resultDto = MeetingMapper.ToDto(meeting, _baseUrl);
                return new SuccessDataResult<MeetingDto>(resultDto, "Toplantı getirildi.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Toplantı getirilirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> GetAllMeetingsAsync(Guid userId)
        {
            try
            {
                var meetings = await _unitOfWork.Meetings.GetByUserIdAsync(userId);
                var resultDtos = MeetingMapper.ToDtoList(meetings, _baseUrl).ToList();

                return new SuccessDataResult<List<MeetingDto>>(resultDtos, $"{resultDtos.Count} toplantı getirildi.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Toplantılar getirilirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> GetUpcomingMeetingsAsync(Guid userId)
        {
            try
            {
                var meetings = await _unitOfWork.Meetings.GetUpcomingMeetingsAsync(userId);
                var resultDtos = MeetingMapper.ToDtoList(meetings, _baseUrl).ToList();

                return new SuccessDataResult<List<MeetingDto>>(resultDtos, $"{resultDtos.Count} gelecek toplantı getirildi.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Gelecek toplantılar getirilirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> GetPastMeetingsAsync(Guid userId)
        {
            try
            {
                var meetings = await _unitOfWork.Meetings.GetPastMeetingsAsync(userId);
                var resultDtos = MeetingMapper.ToDtoList(meetings, _baseUrl).ToList();

                return new SuccessDataResult<List<MeetingDto>>(resultDtos, $"{resultDtos.Count} geçmiş toplantı getirildi.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Geçmiş toplantılar getirilirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> CancelMeetingAsync(Guid meetingId, Guid userId)
        {
            try
            {
                var meeting = await _unitOfWork.Meetings.GetByIdWithDetailsAsync(meetingId);
                if (meeting == null)
                {
                    return new ErrorResult("Toplantı bulunamadı.");
                }

                if (meeting.UserId != userId)
                {
                    return new ErrorResult("Bu toplantıyı iptal etme yetkiniz yok.");
                }

                if (meeting.IsCancelled)
                {
                    return new ErrorResult("Toplantı zaten iptal edilmiş.");
                }

                if (meeting.EndDate < DateTime.UtcNow)
                {
                    return new ErrorResult("Geçmiş toplantılar iptal edilemez.");
                }

                meeting.IsCancelled = true;
                meeting.CancelledAt = DateTime.UtcNow;
                meeting.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Meetings.Update(meeting);
                await _unitOfWork.SaveChangesAsync();

                return new SuccessResult("Toplantı başarıyla iptal edildi.");
            }
            catch (DbUpdateException ex)
            {
                return new ErrorResult($"Veritabanı hatası: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Toplantı iptal edilirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> GetCancelledMeetingsAsync(Guid userId)
        {
            try
            {
                var meetings = await _unitOfWork.Meetings
                    .GetAllQueryable()
                    .Where(m => m.UserId == userId && m.IsCancelled)
                    .Include(m => m.Documents)
                    .OrderByDescending(m => m.CancelledAt)
                    .ToListAsync();

                var resultDtos = MeetingMapper.ToDtoList(meetings, _baseUrl).ToList();

                return new SuccessDataResult<List<MeetingDto>>(resultDtos, $"{resultDtos.Count} iptal edilmiş toplantı getirildi.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"İptal edilmiş toplantılar getirilirken bir hata oluştu: {ex.Message}");
            }
        }
    }
}