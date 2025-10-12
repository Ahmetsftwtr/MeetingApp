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
        private readonly string _appUrl;

        public EmailService(IBus bus, IConfiguration configuration)
        {
            _bus = bus;
            _appUrl = configuration["AppSettings:BaseUrl"] ?? "https://localhost:7285";
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
                    { "AppUrl", _appUrl },
                    { "RegisterDate", DateTime.Now.ToString("dd MMMM yyyy HH:mm") }
                }
            };

            _bus.Send(emailMessage);
            Console.WriteLine($"📨 Email kuyruğa eklendi: {toEmail}");
        }
    }
}