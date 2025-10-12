using System;
using System.Collections.Generic;
using System.Text;

namespace MeetingApp.Models.DTOs.Meeting
{
    public class MeetingDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsCancelled { get; set; }
        public DateTime? CancelledAt { get; set; }

        public Guid UserId { get; set; }

        public List<MeetingDocumentDto> Documents { get; set; } = new();
    }
}
