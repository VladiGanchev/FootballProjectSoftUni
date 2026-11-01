using FootballProjectSoftUni.Core.Models.Tournament;

namespace FootballProjectSoftUni.Core.Contracts.Tournament
{
    /// <summary>
    /// What happens when a tournament starts: with fewer than the minimum number of teams it is cancelled
    /// (money returned), otherwise teams without an opponent go through to the next round.
    /// </summary>
    public interface ITournamentStartService
    {
        /// <summary>Tournaments starting within 24 hours with too few teams; each one is returned only once.</summary>
        Task<IReadOnlyList<LowTeamsWarning>> TakeLowTeamsWarningsAsync();

        /// <summary>Handles the tournaments whose start time has come.</summary>
        Task<IReadOnlyList<TournamentStartResult>> StartDueTournamentsAsync();
    }
}
