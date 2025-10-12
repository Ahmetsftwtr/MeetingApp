using MeetingApp.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MeetingApp.Models.Entities
{
    public class MeetingDocument
    {
        public Guid Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string OriginalFileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public string? FileExtension { get; set; }

        public long FileSize { get; set; }  // byte türünden

        public string? ContentType { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public Guid MeetingId { get; set; }
        public Meeting Meeting { get; set; } = null!;
    }
}
