using FootballProjectSoftUni.Core.Models.Coach;
using FootballProjectSoftUni.Core.Models.ServiceError;
using FootballProjectSoftUni.Core.Models.Tournament;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Contracts.Coach
{
    public interface ICoachService
    {
        Task BecomeCoachAsync(CoachViewModel model, string id);
        Task<IEnumerable<TournamentViewModel>> GetAllTournamentsToParticipateAsCoachAsync(string id);
        Task<bool> LeaveTournamentAsync(int id, string Id);
        Task<ServiceError> CheckForErrorsAsync(string userId);

    }
}
