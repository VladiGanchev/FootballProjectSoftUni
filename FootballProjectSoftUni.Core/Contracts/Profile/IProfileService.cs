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
        Task RemoveRefereeRoleAsync(string userId);
    }
}
