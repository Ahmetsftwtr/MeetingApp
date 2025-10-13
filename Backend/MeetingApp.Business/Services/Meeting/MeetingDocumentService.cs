using MeetingApp.Business.Abstractions.File;
using MeetingApp.Business.Abstractions.Meeting;
using MeetingApp.Business.Mappings;
using MeetingApp.DataAccess.Repositories.UnitOfWork;
using MeetingApp.Models.DTOs.File;
using MeetingApp.Models.DTOs.Meeting;
using MeetingApp.Models.Entities;
using MeetingApp.Models.ReturnTypes.Abstract;
using MeetingApp.Models.ReturnTypes.Concrete;
using Microsoft.Extensions.Configuration;

namespace MeetingApp.Business.Services.Meeting
{
    public class MeetingDocumentService : IMeetingDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly string _baseUrl;
        private readonly string _uploadPath;
        private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".jpg", ".jpeg", ".png" };
        private const long MaxFileSize = 10 * 1024 * 1024;

        public MeetingDocumentService(IUnitOfWork unitOfWork,IFileService fileService, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _baseUrl = configuration["AppSettings:BaseUrl"] ?? "https://localhost:5001";
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "meetings");

            if (!Directory.Exists(_uploadPath))
                Directory.CreateDirectory(_uploadPath);
        }

        public async Task<IResult> UploadDocumentAsync(Guid meetingId, Guid userId, FileUploadDto dto)
        {
            try
            {
                var meeting = await _unitOfWork.Meetings.GetByIdWithDetailsAsync(meetingId);
                if (meeting == null)
                    return new ErrorResult("Toplantı bulunamadı.");

                if (meeting.UserId != userId)
                    return new ErrorResult("Bu toplantıya döküman yükleme yetkiniz yok.");

                var uploadResult = await _fileService.UploadFileAsync(dto, "meeting-documents", meetingId.ToString());
                if (!uploadResult.IsSuccess)
                    return uploadResult;

                var uploadData = ((SuccessDataResult<FileUploadResultDto>)uploadResult).Data;

                var document = new MeetingDocument
                {
                    Id = Guid.NewGuid(),
                    FileName = uploadData.FileName,
                    OriginalFileName = uploadData.OriginalFileName,
                    FilePath = uploadData.FilePath,
                    FileExtension = uploadData.FileExtension,
                    FileSize = uploadData.FileSize,
                    ContentType = uploadData.ContentType,
                    MeetingId = meetingId,
                    UploadedAt = DateTime.UtcNow
                };

                await _unitOfWork.MeetingDocuments.AddAsync(document);
                await _unitOfWork.SaveChangesAsync();

                var resultDto = MeetingDocumentMapper.ToDto(document, _baseUrl);
                return new SuccessDataResult<MeetingDocumentDto>(resultDto, "Döküman başarıyla yüklendi.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Döküman yüklenirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> GetDocumentsByMeetingIdAsync(Guid meetingId, Guid userId)
        {
            try
            {
                var meeting = await _unitOfWork.Meetings.GetByIdWithDetailsAsync(meetingId);
                if (meeting == null)
                    return new ErrorResult("Toplantı bulunamadı.");

                if (meeting.UserId != userId)
                    return new ErrorResult("Bu toplantının dökümanlarını görme yetkiniz yok.");

                var documents = await _unitOfWork.MeetingDocuments.GetByMeetingIdAsync(meetingId);
                var resultDtos = documents.Select(d => MeetingDocumentMapper.ToDto(d, _baseUrl)).ToList();

                return new SuccessDataResult<List<MeetingDocumentDto>>(resultDtos, $"{resultDtos.Count} döküman getirildi.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Dökümanlar getirilirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> DownloadDocumentAsync(Guid documentId, Guid userId)
        {
            try
            {
                var document = await _unitOfWork.MeetingDocuments.GetByIdAsync(documentId);
                if (document == null)
                    return new ErrorResult("Döküman bulunamadı.");

                var meeting = await _unitOfWork.Meetings.GetByIdWithDetailsAsync(document.MeetingId);
                if (meeting?.UserId != userId)
                    return new ErrorResult("Bu dökümanı indirme yetkiniz yok.");

                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", document.FilePath);
                if (!System.IO.File.Exists(fullPath))
                    return new ErrorResult("Dosya sunucuda bulunamadı.");

                var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);

                return new SuccessDataResult<(byte[] bytes, string contentType, string fileName)>(
                    (fileBytes, document.ContentType ?? "application/octet-stream", document.OriginalFileName),
                    "Döküman indirildi."
                );
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Döküman indirilirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> DeleteDocumentAsync(Guid documentId, Guid userId)
        {
            try
            {
                var document = await _unitOfWork.MeetingDocuments.GetByIdAsync(documentId);
                if (document == null)
                    return new ErrorResult("Döküman bulunamadı.");

                var meeting = await _unitOfWork.Meetings.GetByIdWithDetailsAsync(document.MeetingId);
                if (meeting?.UserId != userId)
                    return new ErrorResult("Bu dökümanı silme yetkiniz yok.");

                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", document.FilePath);
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);

                _unitOfWork.MeetingDocuments.Remove(document);
                await _unitOfWork.SaveChangesAsync();

                return new SuccessResult("Döküman başarıyla silindi.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Döküman silinirken bir hata oluştu: {ex.Message}");
            }
        }
    }
}