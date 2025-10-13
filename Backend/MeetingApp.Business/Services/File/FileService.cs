using MeetingApp.Business.Abstractions.File;
using MeetingApp.Models.DTOs.File;
using MeetingApp.Models.ReturnTypes.Abstract;
using MeetingApp.Models.ReturnTypes.Concrete;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingApp.Business.Services.File
{
    public class FileService : IFileService
    {
        private readonly string _baseUrl;
        private readonly string _uploadsBasePath;
        private readonly Dictionary<string, string[]> _allowedExtensions;
        private readonly Dictionary<string, long> _maxFileSizes;

        public FileService(IConfiguration configuration)
        {
            _baseUrl = configuration["AppSettings:BaseUrl"] ?? "http://localhost:7000";
            _uploadsBasePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            _allowedExtensions = new Dictionary<string, string[]>
            {
                ["profile"] = new[] { ".jpg", ".jpeg", ".png", ".gif" },
                ["meeting-documents"] = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".jpg", ".jpeg", ".png" }
            };

            _maxFileSizes = new Dictionary<string, long>
            {
                ["profile"] = 10 * 1024 * 1024, // 5 MB
                ["meeting-documents"] = 100 * 1024 * 1024 // 10 MB
            };
        }

        public async Task<IResult> UploadFileAsync(FileUploadDto dto, string category, string? subFolder = null)
        {
            try
            {
                if (dto.FileBytes == null || dto.FileBytes.Length == 0)
                    return new ErrorResult("Dosya seçilmedi.");

                if (!_allowedExtensions.ContainsKey(category))
                    return new ErrorResult("Geçersiz dosya kategorisi.");

                var extension = Path.GetExtension(dto.OriginalFileName).ToLowerInvariant();
                if (!_allowedExtensions[category].Contains(extension))
                    return new ErrorResult($"Geçersiz dosya tipi. İzin verilen: {string.Join(", ", _allowedExtensions[category])}");

                if (dto.FileSize > _maxFileSizes[category])
                    return new ErrorResult($"Dosya boyutu {_maxFileSizes[category] / (1024 * 1024)} MB'dan büyük olamaz.");

                var uploadPath = Path.Combine(_uploadsBasePath, category);
                if (!string.IsNullOrEmpty(subFolder))
                    uploadPath = Path.Combine(uploadPath, subFolder);

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadPath, uniqueFileName);

                await System.IO.File.WriteAllBytesAsync(filePath, dto.FileBytes);

                var relativePath = Path.Combine("uploads", category, subFolder ?? "", uniqueFileName).Replace("\\", "/");
                var fileUrl = $"{_baseUrl}/{relativePath}";

                var result = new FileUploadResultDto
                {
                    FileName = uniqueFileName,
                    OriginalFileName = dto.OriginalFileName,
                    FilePath = relativePath,
                    FileUrl = fileUrl,
                    FileExtension = extension,
                    FileSize = dto.FileSize,
                    ContentType = dto.ContentType
                };

                return new SuccessDataResult<FileUploadResultDto>(result, "Dosya başarıyla yüklendi.");
            }
            catch (IOException ex)
            {
                return new ErrorResult($"Dosya kaydedilemedi: {ex.Message}");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Dosya yüklenirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<bool> DeleteFileAsync(string relativePath)
        {
            try
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public string GetFileUrl(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return $"{_baseUrl}/uploads/default-avatar.png";

            return $"{_baseUrl}/{relativePath}";
        }
    }
}