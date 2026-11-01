namespace FootballProjectSoftUni.Core.Models.Notification
{
    /// <summary>The notification and e-mail texts of an announcement in one language.</summary>
    public class AnnouncementText
    {
        /// <summary>
        /// Stands in the e-mail where each person's own "unsubscribe" link goes (e-mails about new tournaments).
        /// </summary>
        public const string UnsubscribeLinkPlaceholder = "%UNSUBSCRIBE_LINK%";

        public AnnouncementText(string subject, string notification, string emailHtml)
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
