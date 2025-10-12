using MeetingApp.Business.Abstractions.Email;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace MeetingApp.Business.Services.Email
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public SmtpEmailSender(IConfiguration config)
        {
            _fromEmail = config["EmailSettings:SenderEmail"]!;
            _fromName = config["EmailSettings:SenderName"]!;
            _smtpHost = config["EmailSettings:SmtpHost"]!;
            _smtpPort = int.Parse(config["EmailSettings:SmtpPort"]!);
            _smtpUsername = config["EmailSettings:Username"]!;
            _smtpPassword = config["EmailSettings:Password"]!;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            using var client = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true,
                SubjectEncoding = Encoding.UTF8,
                HeadersEncoding = Encoding.UTF8
            };
            mail.To.Add(to);

            await client.SendMailAsync(mail);
        }
    }
}