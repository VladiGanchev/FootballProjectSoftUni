using FootballProjectSoftUni.Core.Models.ServiceError;
using FootballProjectSoftUni.Core.Models.Team;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Contracts.Team
{
    public interface ITeamService
    {
        Task<ServiceError> CheckForErrorsAsync(int id, string userId);
        Task<int> GetCityIdAsync(int id);
        TeamRegistrationViewModel CreateModel(int id);

        Task<ServiceError> JoinTeamAsync(TeamRegistrationViewModel viewModel, int id, string UserId);

        Task<int> CreateTeamDraftAsync(TeamRegistrationViewModel viewModel, string userId);
        Task FinalizeJoinAsync(int tournamentId, string userId, int teamId, int cityId);
        Task<int?> GetCoachTeamIdAsync(string userId);
    }
}
