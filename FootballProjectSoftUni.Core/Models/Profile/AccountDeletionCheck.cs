using FootballProjectSoftUni.Core.Extensions;
namespace FootballProjectSoftUni.Core.Models.Profile
{
    public enum AccountDeletionBlock
    {
        None,

        /// <summary>Administrators / tournament organisers cannot delete their account.</summary>
        Organizer,

        /// <summary>The coach's team is in a tournament that has not finished yet.</summary>
        CoachInTournament,

        /// <summary>The user referees a tournament that has not finished yet.</summary>
        RefereeInTournament
    }

    /// <summary>Whether a user may delete their account now, and if not - why.</summary>
    public class AccountDeletionCheck
    {
        public AccountDeletionBlock Block { get; set; }

        public bool CanDelete => Block == AccountDeletionBlock.None;

        /// <summary>The tournament that blocks the deletion (coach / referee).</summary>
        public string? TournamentCity { get; set; }

        public DateTime? TournamentStart { get; set; }

        public DateTime? TournamentEnd { get; set; }

        /// <summary>Not started yet: the user can leave it first. Started: wait until it ends.</summary>
        public bool TournamentStarted => TournamentStart.HasValue && BgTime.Now >= TournamentStart.Value;
    }
}
