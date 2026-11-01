using FootballProjectSoftUni.Core.Contracts.Email;
using Hangfire;

namespace FootballProjectSoftUni.Services
{
    /// <summary>One background job per e-mail: if it fails, Hangfire retries just this one.</summary>
    public class EmailJob
    {
        private readonly IEmailService emailService;

        public EmailJob(IEmailService emailService)
        {
            this.emailService = emailService;
        }

        [AutomaticRetry(Attempts = 5)]
        public Task SendAsync(string to, string subject, string htmlBody) =>
            emailService.SendAsync(to, subject, htmlBody);
    }

    public class HangfireEmailQueue : IEmailQueue
    {
        private readonly IBackgroundJobClient jobs;

        public HangfireEmailQueue(IBackgroundJobClient jobs)
        {
            this.jobs = jobs;
        }

        public void Enqueue(string to, string subject, string htmlBody) =>
            jobs.Enqueue<EmailJob>(job => job.SendAsync(to, subject, htmlBody));
    }
}
