namespace FootballProjectSoftUni.Core.Contracts.Email
{
    /// <summary>
    /// Sends an e-mail in the background. Each e-mail is retried on its own, so one failing address
    /// (full mailbox, typo, Gmail's daily limit) never stops or repeats the others.
    /// </summary>
    public interface IEmailQueue
    {
        void Enqueue(string to, string subject, string htmlBody);
    }
}
