namespace FootballProjectSoftUni.Core.Models.Payment
{
    /// <summary>What the coach is about to pay for: shown above the declaration, before Stripe.</summary>
    public class JoinSummaryViewModel
    {
        public string Description { get; set; } = string.Empty;
        public string? CityName { get; set; }
        public DateTime StartDate { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public decimal Fee { get; set; }

        /// <summary>The coach already paid for this tournament (left and came back): no new charge.</summary>
        public bool AlreadyPaid { get; set; }
    }
}
