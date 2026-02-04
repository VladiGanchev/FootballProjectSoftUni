using FootballProjectSoftUni.Core.Contracts.TournamentReminder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Job
{
    public class TournamentReminderJob
    {
        private readonly ITournamentReminderService reminderService;

        public TournamentReminderJob(ITournamentReminderService reminderService)
        {
            this.reminderService = reminderService;
        }

        public Task SendRemindersAsync()
            => reminderService.Send24HourRemindersAsync();
    }



}
