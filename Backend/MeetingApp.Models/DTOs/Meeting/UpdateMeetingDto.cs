using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MeetingApp.Models.DTOs.Meeting
{
    public class UpdateMeetingDto
    {
        [Required(ErrorMessage = "Toplantı adı gereklidir")]
        [StringLength(200, ErrorMessage = "Toplantı adı en fazla 200 karakter olabilir")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Açıklama en fazla 2000 karakter olabilir")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Başlangıç tarihi gereklidir")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Bitiş tarihi gereklidir")]
        public DateTime EndDate { get; set; }
    }
}
