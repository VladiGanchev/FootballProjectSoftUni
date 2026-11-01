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
        private readonly IEmailQueue emailQueue;

        public EmailSenderService(IEmailQueue emailQueue)
            => this.emailQueue = emailQueue;

        // Queued, not sent here: a slow or failing mail server must not break registration or a password reset.
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            emailQueue.Enqueue(email, subject, htmlMessage);
            return Task.CompletedTask;
        }
    }
}
