﻿using FootballProjectSoftUni.Core.Models.City;
using FootballProjectSoftUni.Core.Models.Tournament;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Contracts.Tournament
{
    public interface ITournamentService
    {
        Task<IEnumerable<TournamentViewModel>> GetCityTournamentsAsync(int cityId);
        Task AddTournamentToCityAsync(AddTournamentFormViewModel model, int cityId, DateTime start, DateTime end);
        Task<DetailsViewModel> GetTournamentDetailsAsync(int id);
        Task EditTournamentAsync(EditViewModel model, DateTime start, DateTime end);
        Task<bool> DeleteTournamentAsync(int id);
        Task<IEnumerable<CityViewModel>> GetCitiesAsync();
        Task<CityViewModel> FindCityAsync(int cityId);
        Task<EditViewModel> FindTournamentAsync(int cityId);
        Task<FootballProjectSoftUni.Infrastructure.Data.Models.Tournament> FindTournamentByIdAsync(int id);
    }
}
