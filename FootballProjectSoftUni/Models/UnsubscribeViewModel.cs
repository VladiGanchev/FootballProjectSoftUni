namespace FootballProjectSoftUni.Models
{
    /// <summary>The page behind the "unsubscribe" link of the e-mails about new tournaments.</summary>
    public class UnsubscribeViewModel
    {
        /// <summary>The account (user id) from the link.</summary>
        public string? U { get; set; }

        /// <summary>The key from the link.</summary>
        public string? K { get; set; }

        public bool Valid { get; set; }

        public bool Done { get; set; }
    }
}
