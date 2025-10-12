using System;
using System.Collections.Generic;
using System.Text;

namespace MeetingApp.Models.DTOs.Meeting
{
    public class MeetingDocumentDto
    {
        public Guid Id { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
        public string FileSize { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }
}
