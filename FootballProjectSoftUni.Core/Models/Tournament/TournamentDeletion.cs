namespace FootballProjectSoftUni.Core.Models.Tournament
{
    public enum TournamentDeletionStatus
    {
        Deleted,
        NotFound,

        /// <summary>A tournament is in progress: it can be deleted only after it ends.</summary>
        Running,

        /// <summary>Some paid teams could not be refunded, so nothing was deleted.</summary>
        RefundsFailed,

        /// <summary>The refunds succeeded but the database deletion failed (rolled back); see Error.</summary>
        DeleteFailed
    }

    /// <summary>What the admin sees before confirming the deletion of a tournament or a city.</summary>
    public class TournamentDeletionPreview
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public int? CityId { get; set; }

        public string? CityName { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        /// <summary>The tournament (or, for a city, one of its tournaments) is in progress.</summary>
        public bool IsRunning { get; set; }

        /// <summary>Deletion is blocked until this moment.</summary>
        public DateTime? RunningUntil { get; set; }

        public bool IsFinished { get; set; }

        /// <summary>Paid teams of tournaments that have not started yet - they get their money back.</summary>
        public int PaidTeams { get; set; }

        public decimal PaidTotal { get; set; }
    }

    /// <summary>A coach whose fee was returned because the tournament was deleted before it started.</summary>
    public class RefundedTeam
    {
        public string UserId { get; set; } = null!;

        public string Culture { get; set; } = "bg";

        public decimal Amount { get; set; }

        public string? City { get; set; }

        public DateTime StartDate { get; set; }
    }

    /// <summary>Another participant (referee, coach without payment) of a tournament deleted before it started.</summary>
    public class CanceledParticipant
    {
        public string UserId { get; set; } = null!;

        public string? City { get; set; }

        public DateTime StartDate { get; set; }
    }

    public class TournamentDeletionResult
    {
        public TournamentDeletionStatus Status { get; set; }

        public int? CityId { get; set; }

        public DateTime? RunningUntil { get; set; }

        /// <summary>Teams whose money was returned (also when the deletion stopped because of other failures).</summary>
        public List<RefundedTeam> Refunded { get; } = new();

        public List<CanceledParticipant> OtherParticipants { get; } = new();

        /// <summary>Teams that could not be refunded.</summary>
        public List<string> FailedTeams { get; } = new();

        public List<int> DeletedTournamentIds { get; } = new();

        /// <summary>The pictures of the deleted city and tournaments (removed from the site afterwards).</summary>
        public List<string> DeletedPictureUrls { get; } = new();

        public Exception? Error { get; set; }
    }
}
