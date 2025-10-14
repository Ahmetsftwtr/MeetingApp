using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingApp.Models.DTOs.Meeting
{
    public class MeetingFilterDto
    {
        public MeetingStatus? Status { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }
        public bool? IsCancelled { get; set; }
        public string? SearchTerm { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string? OrderBy { get; set; } = "StartDate";
        public bool IsDescending { get; set; } = true;
    }

    public enum MeetingStatus
    {
        All = 0,
        Upcoming = 1,
        Past = 2,
        Cancelled = 3,
        Active = 4 
    }
}
