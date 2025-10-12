using MeetingApp.Models.Entities;

namespace MeetingApp.Model.Entities
{
    public class Meeting
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? Description { get; set; }

        public string? DocumentPath { get; set; }

        public bool IsCancelled { get; set; } = false;
        public DateTime? CancelledAt { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public ICollection<MeetingDocument> Documents { get; set; } = new List<MeetingDocument>();

    }
}
