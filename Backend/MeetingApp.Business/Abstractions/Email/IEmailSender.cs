using MeetingApp.Models.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace MeetingApp.Business.Abstractions.Email
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
    }
}
