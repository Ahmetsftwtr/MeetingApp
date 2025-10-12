using System;
using System.Collections.Generic;
using System.Text;

namespace MeetingApp.Business.Abstractions.Email
{
    public interface IEmailTemplateService
    {
        string RenderTemplate(string templateName, Dictionary<string, string> data);
    }
}
