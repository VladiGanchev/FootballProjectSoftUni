using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Contracts.TournamentReminder
{
    public interface ITournamentReminderService
    {
        Task Send24HourRemindersAsync();
    }
}
