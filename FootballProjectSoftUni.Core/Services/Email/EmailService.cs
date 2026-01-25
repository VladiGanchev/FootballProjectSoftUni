using FootballProjectSoftUni.Core.Contracts.Email;
using FootballProjectSoftUni.Core.Models.Email;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings settings;

        public EmailService(IOptions<EmailSettings> options)
        {
            settings = options.Value;
        }

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            var recipient = string.IsNullOrWhiteSpace(settings.OverrideTo) ? to : settings.OverrideTo;

            using var message = new MailMessage();
            message.From = new MailAddress(settings.SmtpUser, "FootballProject");
            message.To.Add(recipient!);
            message.Subject = subject;
            message.Body = htmlBody;
            message.IsBodyHtml = true;

            using var client = new SmtpClient(settings.SmtpHost, settings.SmtpPort)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(settings.SmtpUser, settings.SmtpPass),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            await client.SendMailAsync(message);
        }
    }
}


