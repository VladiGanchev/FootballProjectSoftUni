using FootballProjectSoftUni.Core.Contracts.TournamentReminder;
using Hangfire;

namespace FootballProjectSoftUni.Services
{
    /// <summary>Every 30 minutes (Hangfire): reminders for tournaments that start within 24 hours.</summary>
    public class TournamentReminderJob
    {
        private readonly ITournamentReminderService reminderService;

        public TournamentReminderJob(ITournamentReminderService reminderService)
        {
            this.reminderService = reminderService;
        }

        // Never two runs at the same time (a slow run must not overlap the next one).
        [DisableConcurrentExecution(timeoutInSeconds: 300)]
        public Task SendRemindersAsync()
            => reminderService.Send24HourRemindersAsync();
    }
}
