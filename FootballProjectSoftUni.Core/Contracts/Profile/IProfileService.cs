using FootballProjectSoftUni.Core.Models.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Contracts.Profile
{
    public interface IProfileService
    {
        Task<ProfileViewModel?> GetProfileAsync(string userId);
        /// <summary>Switches the referee profile off (ratings stay). Not allowed during an unfinished tournament.</summary>
        Task<FootballProjectSoftUni.Core.Models.ServiceError.ServiceError?> RemoveRefereeRoleAsync(string userId);

        /// <summary>Turns the e-mails about new tournaments on or off (from the profile).</summary>
        Task SetNewTournamentEmailsAsync(string userId, bool on);

        /// <summary>Whether the key of an "unsubscribe" link belongs to this account.</summary>
        Task<bool> IsUnsubscribeKeyValidAsync(string? userId, string? key);

        /// <summary>Turns the e-mails about new tournaments off from the link in such an e-mail (no sign-in).</summary>
        /// <returns>False when the link is not valid (any more).</returns>
        Task<bool> UnsubscribeAsync(string? userId, string? key);
    }
}
