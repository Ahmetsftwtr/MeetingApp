using MeetingApp.Business.Abstractions.Email;
using MeetingApp.Models.Messages;
using Rebus.Handlers;
using System;
using System.Threading.Tasks;

namespace MeetingApp.Business.Handlers.Email
{
    public class EmailMessageHandler : IHandleMessages<EmailMessage>
    {
        private readonly IEmailTemplateService _templateService;
        private readonly IEmailSender _emailSender;

        public EmailMessageHandler(
            IEmailTemplateService templateService,
            IEmailSender emailSender)
        {
            _templateService = templateService;
            _emailSender = emailSender;
        }

        public async Task Handle(EmailMessage message)
        {
            try
            {

                var body = _templateService.RenderTemplate(message.TemplateName, message.TemplateData);

                await _emailSender.SendEmailAsync(message.ToEmail, message.Subject, body);

            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}