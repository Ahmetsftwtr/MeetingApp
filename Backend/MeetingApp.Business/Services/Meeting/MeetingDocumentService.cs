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
using Microsoft.Extensions.Logging;

namespace MeetingApp.Business.Services.Meeting
{
    public class MeetingDocumentService : IMeetingDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly ILogger<MeetingDocumentService> _logger;
        private readonly string _baseUrl;

        public MeetingDocumentService(
            IUnitOfWork unitOfWork,
            IFileService fileService,
            IConfiguration configuration,
            ILogger<MeetingDocumentService> logger)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _logger = logger;
            _baseUrl = configuration["AppSettings:BaseUrl"] ?? "http://localhost:7000";
        }

        public async Task<IResult> UploadDocumentAsync(Guid meetingId, Guid userId, FileUploadDto dto)
        {
            string? uploadedFilePath = null;

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Document upload started for meeting: {MeetingId} by user: {UserId}", meetingId, userId);

                var meeting = await _unitOfWork.Meetings.GetByIdWithDetailsAsync(meetingId);
                if (meeting == null)
                {
                    _logger.LogWarning("Document upload failed: Meeting not found - {MeetingId}", meetingId);
                    return new ErrorResult("Toplantı bulunamadı.");
                }

                if (meeting.UserId != userId)
                {
                    _logger.LogWarning(
                        "Document upload failed: Unauthorized access attempt by user {UserId} to meeting {MeetingId}",
                        userId,
                        meetingId
                    );
                    return new ErrorResult("Bu toplantıya döküman yükleme yetkiniz yok.");
                }

                _logger.LogInformation("Uploading document: {FileName} for meeting: {MeetingId}", dto.OriginalFileName, meetingId);

                var uploadResult = await _fileService.UploadFileAsync(dto, "meeting-documents", meetingId.ToString());
                if (!uploadResult.IsSuccess)
                {
                    _logger.LogWarning("Document upload failed for meeting {MeetingId}: {Message}", meetingId, uploadResult.Message);
                    return uploadResult;
                }

                var uploadData = ((SuccessDataResult<FileUploadResultDto>)uploadResult).Data;
                uploadedFilePath = uploadData.FilePath;

                _logger.LogInformation("Document uploaded successfully: {FilePath}", uploadedFilePath);

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

                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Document saved successfully: {DocumentId} for meeting: {MeetingId}",
                    document.Id,
                    meetingId
                );

                var resultDto = MeetingDocumentMapper.ToDto(document, _baseUrl);
                return new SuccessDataResult<MeetingDocumentDto>(resultDto, "Döküman başarıyla yüklendi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during document upload for meeting: {MeetingId}", meetingId);

                await transaction.RollbackAsync();

                if (uploadedFilePath != null)
                {
                    try
                    {
                        _logger.LogInformation(
                            "Cleaning up uploaded document due to upload failure: {FilePath}",
                            uploadedFilePath
                        );
                        await _fileService.DeleteFileAsync(uploadedFilePath);
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogError(cleanupEx, "Failed to cleanup uploaded document: {FilePath}", uploadedFilePath);
                    }
                }

                return new ErrorResult($"Döküman yüklenirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> GetDocumentsByMeetingIdAsync(Guid meetingId, Guid userId)
        {
            try
            {
                _logger.LogInformation("Fetching documents for meeting: {MeetingId} by user: {UserId}", meetingId, userId);

                var meeting = await _unitOfWork.Meetings.GetByIdWithDetailsAsync(meetingId);
                if (meeting == null)
                {
                    _logger.LogWarning("Get documents failed: Meeting not found - {MeetingId}", meetingId);
                    return new ErrorResult("Toplantı bulunamadı.");
                }

                if (meeting.UserId != userId)
                {
                    _logger.LogWarning(
                        "Get documents failed: Unauthorized access attempt by user {UserId} to meeting {MeetingId}",
                        userId,
                        meetingId
                    );
                    return new ErrorResult("Bu toplantının dökümanlarını görme yetkiniz yok.");
                }

                var documents = await _unitOfWork.MeetingDocuments.GetByMeetingIdAsync(meetingId);
                var resultDtos = documents.Select(d => MeetingDocumentMapper.ToDto(d, _baseUrl)).ToList();

                _logger.LogInformation("Found {Count} documents for meeting: {MeetingId}", resultDtos.Count, meetingId);

                return new SuccessDataResult<List<MeetingDocumentDto>>(resultDtos, $"{resultDtos.Count} döküman getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching documents for meeting: {MeetingId}", meetingId);
                return new ErrorResult($"Dökümanlar getirilirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> DownloadDocumentAsync(Guid documentId, Guid userId)
        {
            try
            {
                _logger.LogInformation("Document download requested: {DocumentId} by user: {UserId}", documentId, userId);

                var document = await _unitOfWork.MeetingDocuments.GetByIdAsync(documentId);
                if (document == null)
                {
                    _logger.LogWarning("Download failed: Document not found - {DocumentId}", documentId);
                    return new ErrorResult("Döküman bulunamadı.");
                }

                var meeting = await _unitOfWork.Meetings.GetByIdWithDetailsAsync(document.MeetingId);
                if (meeting?.UserId != userId)
                {
                    _logger.LogWarning(
                        "Download failed: Unauthorized access attempt by user {UserId} to document {DocumentId}",
                        userId,
                        documentId
                    );
                    return new ErrorResult("Bu dökümanı indirme yetkiniz yok.");
                }

                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", document.FilePath);
                if (!System.IO.File.Exists(fullPath))
                {
                    _logger.LogError("Download failed: File not found on server - {FilePath}", fullPath);
                    return new ErrorResult("Dosya sunucuda bulunamadı.");
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);

                _logger.LogInformation("Document downloaded successfully: {DocumentId} - {FileName}", documentId, document.OriginalFileName);

                return new SuccessDataResult<(byte[] bytes, string contentType, string fileName)>(
                    (fileBytes, document.ContentType ?? "application/octet-stream", document.OriginalFileName),
                    "Döküman indirildi."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during document download: {DocumentId}", documentId);
                return new ErrorResult($"Döküman indirilirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> DeleteDocumentAsync(Guid documentId, Guid userId)
        {
            try
            {
                _logger.LogInformation("Document deletion requested: {DocumentId} by user: {UserId}", documentId, userId);

                var document = await _unitOfWork.MeetingDocuments.GetByIdAsync(documentId);
                if (document == null)
                {
                    _logger.LogWarning("Delete failed: Document not found - {DocumentId}", documentId);
                    return new ErrorResult("Döküman bulunamadı.");
                }

                var meeting = await _unitOfWork.Meetings.GetByIdWithDetailsAsync(document.MeetingId);
                if (meeting?.UserId != userId)
                {
                    _logger.LogWarning(
                        "Delete failed: Unauthorized access attempt by user {UserId} to document {DocumentId}",
                        userId,
                        documentId
                    );
                    return new ErrorResult("Bu dökümanı silme yetkiniz yok.");
                }

                var filePath = document.FilePath;

                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                    _logger.LogInformation("Physical file deleted: {FilePath}", fullPath);
                }

                _unitOfWork.MeetingDocuments.Remove(document);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Document deleted successfully: {DocumentId}", documentId);

                return new SuccessResult("Döküman başarıyla silindi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during document deletion: {DocumentId}", documentId);
                return new ErrorResult($"Döküman silinirken bir hata oluştu: {ex.Message}");
            }
        }
    }
}