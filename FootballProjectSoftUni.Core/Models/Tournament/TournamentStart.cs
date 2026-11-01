namespace FootballProjectSoftUni.Core.Models.Tournament
{
    public enum TournamentStartOutcome
    {
        /// <summary>Enough teams: the bracket is ready, teams without an opponent went through.</summary>
        Started,

        /// <summary>Too few teams: every paid team got its money back and the tournament was deleted.</summary>
        Canceled,

        /// <summary>Too few teams, but some refunds failed - nothing deleted, it is tried again next time.</summary>
        CancelFailed
    }

    public class TournamentStartResult
    {
        public int TournamentId { get; set; }

        public string? City { get; set; }

        public DateTime StartDate { get; set; }

        public int Teams { get; set; }

        public TournamentStartOutcome Outcome { get; set; }

        public TournamentDeletionResult? Cancellation { get; set; }
    }

    /// <summary>A tournament starting within 24 hours with fewer teams than needed.</summary>
    public class LowTeamsWarning
    {
        public int TournamentId { get; set; }

        public string? City { get; set; }

        public DateTime StartDate { get; set; }

        public int Teams { get; set; }
    }
}
