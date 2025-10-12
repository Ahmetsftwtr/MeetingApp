using MeetingApp.Business.Abstractions.Email;
using System;
using System.Collections.Generic;
using System.IO;

namespace MeetingApp.Business.Services.Email
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly string _templateBasePath;

        public EmailTemplateService()
        {
            _templateBasePath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates");

            if (!Directory.Exists(_templateBasePath))
                Directory.CreateDirectory(_templateBasePath);
        }

        public string RenderTemplate(string templateName, Dictionary<string, string> data)
        {
            var layoutPath = Path.Combine(_templateBasePath, "Layout.html");
            var contentPath = Path.Combine(_templateBasePath, $"{templateName}.html");

            if (!File.Exists(layoutPath))
                throw new FileNotFoundException($"Layout template not found: {layoutPath}");

            if (!File.Exists(contentPath))
                throw new FileNotFoundException($"Email template not found: {contentPath}");

            var layout = File.ReadAllText(layoutPath);
            var content = File.ReadAllText(contentPath);

            foreach (var item in data)
            {
                content = content.Replace($"{{{{{item.Key}}}}}", item.Value);
            }

            layout = layout.Replace("{{Body}}", content);

            foreach (var item in data)
            {
                layout = layout.Replace($"{{{{{item.Key}}}}}", item.Value);
            }

            return layout;
        }
    }
}