using FootballProjectSoftUni.Core.Models.Tournament;

namespace FootballProjectSoftUni.Core.Contracts.Tournament
{
    /// <summary>
    /// Deletes tournaments and cities by the business rules:
    /// not started - the paid teams get their money back first; in progress - not allowed;
    /// finished - deleted without refunds. Payments are always kept as records.
    /// </summary>
    public interface ITournamentDeletionService
    {
        Task<TournamentDeletionPreview?> PreviewTournamentAsync(int tournamentId);

        Task<TournamentDeletionPreview?> PreviewCityAsync(int cityId);

        Task<TournamentDeletionResult> DeleteTournamentAsync(int tournamentId);

        Task<TournamentDeletionResult> DeleteCityAsync(int cityId);

        /// <summary>
        /// Too few teams signed up by the start: every paid team gets its money back (although the start
        /// time has passed - nothing was played) and the tournament is deleted.
        /// </summary>
        Task<TournamentDeletionResult> CancelForTooFewTeamsAsync(int tournamentId);
    }
}
