using FootballProjectSoftUni.Core.Models.Profile;

namespace FootballProjectSoftUni.Core.Contracts.Profile
{
    /// <summary>
    /// "Delete my account" (GDPR): removes the user's personal data by the site's rules.
    /// Payments are kept (accounting records) and a team that played stays in the tournament history.
    /// </summary>
    public interface IAccountDeletionService
    {
        Task<AccountDeletionCheck> CheckAsync(string userId);

        /// <summary>Deletes the account if <see cref="CheckAsync"/> allows it; returns the check either way.</summary>
        Task<AccountDeletionCheck> DeleteAccountAsync(string userId);
    }
}
