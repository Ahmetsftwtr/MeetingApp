using MeetingApp.Business.Abstractions.Email;
using MeetingApp.Models.Messages;
using Microsoft.Extensions.Configuration;
using Rebus.Bus;
using System;
using System.Collections.Generic;

namespace MeetingApp.Business.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly IBus _bus;
        private readonly string _webUrl;

        public EmailService(IBus bus, IConfiguration configuration)
        {
            _bus = bus;
            _webUrl = configuration["AppSettings:WebUrl"] ?? "https://localhost:7285";
        }

        public void QueueWelcomeEmail(string toEmail, string userName)
        {
            var emailMessage = new EmailMessage
            {
                ToEmail = toEmail,
                Subject = "MeetingApp'e Hoş Geldiniz!",
                TemplateName = "WelcomeEmail",
                TemplateData = new Dictionary<string, string>
                {
                    { "UserName", userName },
                    { "AppUrl", _webUrl },
                    { "RegisterDate", DateTime.Now.ToString("dd MMMM yyyy HH:mm") }
                }
            };

            _bus.Send(emailMessage);
            Console.WriteLine($"Email kuyruğa eklendi: {toEmail}");
        }

        public void QueueMeetingCreatedEmail(string toEmail, string userName, Guid meetingId, string meetingTitle, DateTime startDate, DateTime endDate, string description)
        {
            var emailMessage = new EmailMessage
            {
                ToEmail = toEmail,
                Subject = $"Yeni Toplantı: {meetingTitle}",
                TemplateName = "MeetingCreated",
                TemplateData = new Dictionary<string, string>
                {
                    { "UserName", userName },
                    { "MeetingId", meetingId.ToString() },
                    { "MeetingTitle", meetingTitle },
                    { "StartDate", startDate.ToString("dd MMMM yyyy HH:mm") },
                    { "EndDate", endDate.ToString("dd MMMM yyyy HH:mm") },
                    { "Description", description },
                    { "AppUrl", _webUrl },
                    { "CurrentYear", DateTime.Now.Year.ToString() }
                }
            };

            _bus.Send(emailMessage);
            Console.WriteLine($"Toplantı bilgilendirme emaili kuyruğa eklendi: {toEmail}");
        }
    }


}