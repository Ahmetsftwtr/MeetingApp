using System;
using System.Collections.Generic;
using System.Text;

namespace MeetingApp.Models.Messages
{
    public class EmailMessage
    {
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public Dictionary<string, string> TemplateData { get; set; } = new();
    }
}
