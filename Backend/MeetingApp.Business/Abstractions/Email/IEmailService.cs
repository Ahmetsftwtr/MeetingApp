using System;
using System.Collections.Generic;
using System.Text;

namespace MeetingApp.Business.Abstractions.Email
{
    public interface IEmailService
    {
        void QueueWelcomeEmail(string toEmail, string userName);
    }
}