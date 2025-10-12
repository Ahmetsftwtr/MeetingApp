using MeetingApp.Model.Entities;
using MeetingApp.Models.DTOs.Meeting;
using MeetingApp.Models.Entities;
using Microsoft.Extensions.Configuration;

namespace MeetingApp.Business.Mappings
{
    public static class MeetingDocumentMapper
    {
        public static MeetingDocumentDto ToDto(MeetingDocument document, string baseUrl)
        {
            return new MeetingDocumentDto
            {
                Id = document.Id,
                OriginalFileName = document.OriginalFileName,
                Url = $"{baseUrl}/{document.FilePath}", 
                FileExtension = document.FileExtension ?? string.Empty,
                FileSize = FormatFileSize(document.FileSize),
                UploadedAt = document.UploadedAt
            };
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}