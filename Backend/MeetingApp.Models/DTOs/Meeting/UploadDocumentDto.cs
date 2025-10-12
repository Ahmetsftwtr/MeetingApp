using System;
using System.Collections.Generic;
using System.Text;

namespace MeetingApp.Models.DTOs.Meeting
{
    public class UploadDocumentDto
    {
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public byte[] FileBytes { get; set; } = Array.Empty<byte>();
    }
}
