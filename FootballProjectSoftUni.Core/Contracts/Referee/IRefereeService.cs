using FootballProjectSoftUni.Core.Models.Referee;
using FootballProjectSoftUni.Core.Models.ServiceError;
using FootballProjectSoftUni.Core.Models.Tournament;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Contracts.Referee
{
    public interface IRefereeService
    {
        Task<ServiceError> CheckForErrorsAsync(int tournamentId, string userId);
        Task<bool> CreateRefereeToTournamentAsync(RefereeFormViewMOdel model, int id, string userId, DateTime birthdate);
        Task<IEnumerable<TournamentViewModel>> GetTournamentsAsync(string id);
        Task<bool> LeaveTournamentAsync(int id, string userId);
        Task<IEnumerable<RefereeListItemViewModel>> GetAllRefereesWithRatingsAsync(string userId);
        Task RateRefereeAsync(string refereeId, string userId, int rating);
        Task<bool> AssignExistingRefereeToTournamentAsync(string userId, int tournamentId);
        Task<FootballProjectSoftUni.Infrastructure.Data.Models.Referee?> GetRefereeByUserIdAsync(string userId);
    }
}
