using System;
using System.Collections.Generic;
using System.Text;

namespace MeetingApp.Business.Abstractions.Email
{
    public interface IEmailService
    {
        void QueueWelcomeEmail(string toEmail, string userName);

        void QueueMeetingCreatedEmail(string toEmail, string userName, Guid meetingId, string meetingTitle, DateTime startDate, DateTime endDate, string description);
    }
}