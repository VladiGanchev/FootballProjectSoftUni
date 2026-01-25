using FootballProjectSoftUni.Core.Contracts.Email;
using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Services.EmailSender
{
    public class EmailSenderService : IEmailSender
    {
        private readonly IEmailService emailService;

        public EmailSenderService(IEmailService emailService)
            => this.emailService = emailService;

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
            => emailService.SendAsync(email, subject, htmlMessage);
    }
}
