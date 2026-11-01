namespace FootballProjectSoftUni.Core.Contracts.TournamentReminder
{
    /// <summary>The texts of the 24-hour reminder in the participant's language (they live in the web resources).</summary>
    public interface IReminderTexts
    {
        ReminderText For(string culture, string? city, DateTime start);
    }

    public class ReminderText
    {
        public ReminderText(string subject, string notification, string emailHtml)
        {
            Subject = subject;
            Notification = notification;
            EmailHtml = emailHtml;
        }

        public string Subject { get; }

        public string Notification { get; }

        public string EmailHtml { get; }
    }
}
