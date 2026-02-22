using FootballProjectSoftUni.Core.Models.City;
using FootballProjectSoftUni.Core.Models.Match;
using FootballProjectSoftUni.Core.Models.Tournament;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace FootballProjectSoftUni.Core.Contracts.Tournament
{
    public interface ITournamentService
    {
        Task<IPagedList<TournamentViewModel>> GetCityTournamentsAsync(int cityId, bool showPast, int? page);
        Task AddTournamentToCityAsync(AddTournamentFormViewModel model, int cityId, DateTime start, DateTime end);
        Task<DetailsViewModel> GetTournamentDetailsAsync(int id);
        Task EditTournamentAsync(EditViewModel model, DateTime start, DateTime end);
        Task<bool> DeleteTournamentAsync(int id);
        Task<IEnumerable<CityViewModel>> GetCitiesAsync();
        Task<CityViewModel> FindCityAsync(int cityId);
        Task<EditViewModel> FindTournamentAsync(int cityId);
        Task<FootballProjectSoftUni.Infrastructure.Data.Models.Tournament> FindTournamentByIdAsync(int id);
        Task GenerateBracketAsync(int tournamentId);
        Task AssignTeamToBracketAsync(int tournamentId, int teamId);
        Task MoveWinnerToNextRoundAsync(int matchId);
        Task RemoveTeamFromBracketAsync(int tournamentId, int teamId);
        Task<EnterResultViewModel?> GetEnterResultModelAsync(int matchId);
        Task<(bool ok, string? error, int tournamentId)> EnterMatchResultAsync(EnterResultViewModel model);
    }
}
