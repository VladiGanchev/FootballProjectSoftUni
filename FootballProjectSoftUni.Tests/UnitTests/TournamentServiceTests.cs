using FootballProjectSoftUni.Core.Contracts.Team;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Models.City;
using FootballProjectSoftUni.Core.Models.Tournament;
using FootballProjectSoftUni.Core.Services.Team;
using FootballProjectSoftUni.Core.Services.Tournament;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    public class TournamentServiceTests : UnitTestsBase
    {
        private ITournamentService _tournamentService;

        [OneTimeSetUp]

        public void SetUp() => _tournamentService = new TournamentService(_data);

        [Test]

        public async Task FindCity_ShouldBeTrue()
        {
            var model = await _tournamentService.FindCityAsync(2);

            var city = new CityViewModel() 
            {
                Id = 2,
                Name = "Бургас",
            };

            Assert.AreEqual(model.Name, city.Name);
        }

        [Test]

        public async Task FindCity_ShouldBeFalse()
        {
            var model = await _tournamentService.FindCityAsync(33);

            Assert.IsNull(model);
        }

        [Test]
        //Comment
        public async Task GetCities_ShouldBeTrue()
        {
            var cities = await _tournamentService.GetCitiesAsync();

            var cityName1 = new CityViewModel() { Name = "Благоевград" };
            var cityName10 = new CityViewModel() { Name = "Кюстендил" };
            var cityName20 = new CityViewModel() { Name = "Сливен" };

            Assert.AreEqual(cities.First().Name, cityName1.Name);
            Assert.AreEqual(cities.Skip(9).Take(1).First().Name, cityName10.Name);
            Assert.AreEqual(cities.Skip(19).Take(1).First().Name, cityName20.Name);

        }

        [Test]

        public async Task GetDetails_ShouldBeTrue()
        {
            var tournament = new Tournament()
            {
                Id = 35422,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                CreatedOn = DateTime.Now,
                Description = "lkjhgfdvbjkiuytrdvbnjiuytfgbnjuytghjuytg",
                NumberOfTeams = 0,
                Status = FootballProjectSoftUni.Infrastructure.Data.Enums.TournamentStatus.Finished,
                OrganiserId = "600bafb9-a73d-4489-a387-643c3b8ae96c",
                RefereeId = null
            };

            _data.Tournaments.Add(tournament);
            _data.SaveChanges();

            var tc = new TournamentCity()
            {
                TournamentId = tournament.Id,
                CityId = 1
            };

            _data.TournamentsCities.Add(tc);
            _data.SaveChanges();

            var neededModel = await _tournamentService.GetTournamentDetailsAsync(35422);

            var model = new DetailsViewModel()
            {
                Id = 35422,
                Description = "lkjhgfdvbjkiuytrdvbnjiuytfgbnjuytghjuytg",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                RefereeId = null,
                RefereeName = null,
                CreatedOn = DateTime.Now,
                Status = tournament.Status.ToString(),
                NumberOfTeams = 0,
                CityId = 1
            };

            Assert.AreEqual(neededModel.CityId, model.CityId);
        }

        [Test]

        public async Task GetDetails_ShouldBeNull()
        {
            var tournament = new Tournament()
            {
                Id = 35322,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                CreatedOn = DateTime.Now,
                Description = "lkjhgfdvbjkiuytrdvbnjiuytfgbnjuytghjuytg",
                NumberOfTeams = 0,
                Status = FootballProjectSoftUni.Infrastructure.Data.Enums.TournamentStatus.Finished,
                OrganiserId = "600bafb9-a73d-4489-a387-643c3b8ae96c",
                RefereeId = null
            };

            _data.Tournaments.Add(tournament);
            _data.SaveChanges();

            var tc = new TournamentCity()
            {
                TournamentId = tournament.Id,
                CityId = 1
            };

            _data.TournamentsCities.Add(tc);
            _data.SaveChanges();

            var neededModel = await _tournamentService.GetTournamentDetailsAsync(354222);

            var model = new DetailsViewModel()
            {
                Id = 35422,
                Description = "lkjhgfdvbjkiuytrdvbnjiuytfgbnjuytghjuytg",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                RefereeId = null,
                RefereeName = null,
                CreatedOn = DateTime.Now,
                Status = tournament.Status.ToString(),
                NumberOfTeams = 0,
                CityId = 1
            };

            Assert.IsNull(neededModel);
        }


        [Test]

        public async Task FindTournamentByIdAsync_ShouldBeFalse()
        {
            var tournament = await _tournamentService.FindTournamentAsync(7564);

            Assert.IsNull(tournament);
        }

        [Test]

        public async Task FindTournamentByIdAsync_ShouldBeTrue()
        {
            var tournament = new Tournament()
            {
                Id = 5322,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                CreatedOn = DateTime.Now,
                Description = "lkjhgfdvbjkiuytrdvbnjiuytfgbnjuytghjuytg",
                NumberOfTeams = 0,
                Status = FootballProjectSoftUni.Infrastructure.Data.Enums.TournamentStatus.Finished,
                OrganiserId = "600bafb9-a73d-4489-a387-643c3b8ae96c",
                RefereeId = null
            };

            _data.Tournaments.Add(tournament);
            _data.SaveChanges();

            var neededModel = await _tournamentService.FindTournamentAsync(5322);

            var model = new EditViewModel()
            {
                Id = tournament.Id,
                Description = tournament.Description,
                EndDate = tournament.EndDate.ToString(),
                StartDate = tournament.StartDate.ToString(),
                ImageUrl = tournament.ImageUrl,
                CreatedOn = tournament.CreatedOn.ToString()
            };

            Assert.AreEqual(neededModel.Description, model.Description);
        }

        [Test]
        public async Task FindTournamentAsync_ShouldBeTrue()
        {
            var tournament = new Tournament()
            {
                Id = 532,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                CreatedOn = DateTime.Now,
                Description = "lkjhgfdvbjkiuytrdvbnjiuytfgbnjuytghjuytg",
                NumberOfTeams = 0,
                Status = FootballProjectSoftUni.Infrastructure.Data.Enums.TournamentStatus.Finished,
                OrganiserId = "600bafb9-a73d-4489-a387-643c3b8ae96c",
                RefereeId = null
            };

            _data.Tournaments.Add(tournament);
            _data.SaveChanges();

            var neededModel = await _tournamentService.FindTournamentByIdAsync(532);

            Assert.AreEqual(neededModel, tournament);
        }

        [Test]

        public async Task AddTournamentToCity_ShouldBeTrue()
        {
            var tournament = new AddTournamentFormViewModel()
            {
                Description = "ngfdvqwewrgvwgfd",
                ImageUrl = "yrhtervcd"

            };

            await _tournamentService.AddTournamentToCityAsync(tournament, 1, DateTime.Now, DateTime.Now);

            Assert.IsNotNull(_data.Tournaments.Where(x => x.Description == "ngfdvqwewrgvwgfd").FirstOrDefault());

            var neededTournament = _data.Tournaments.Where(x => x.Description == "ngfdvqwewrgvwgfd").FirstOrDefault();

            Assert.IsNotNull(_data.TournamentsCities.Where(x => x.CityId == 1 && x.TournamentId == tournament.Id));
        }

        [Test]
        public async Task GetCityTournamentsAsync_ShouldBeFalse()
        {
            var result = await _tournamentService.GetCityTournamentsAsync(87);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetCityTournamentsAsync_ShouldBeTrue()
        {
            var tournament = new Tournament()
            {
                CreatedOn = DateTime.Now,
                Description = "htgfexdsgfdsabrvcxds",
                EndDate = DateTime.Now,
                StartDate = DateTime.Now,
                ImageUrl = "gevcsxwesfcads",
                NumberOfTeams = 0,
                RefereeId = null,
                Status = Infrastructure.Data.Enums.TournamentStatus.Finished
            };

            _data.Tournaments.Add(tournament);

            _data.SaveChanges();

            var tc = new TournamentCity()
            {
                TournamentId = tournament.Id,
                CityId = 9
            };

            _data.TournamentsCities.Add(tc);

            _data.SaveChanges();

            var result = await _tournamentService.GetCityTournamentsAsync(9);

            Assert.AreEqual(result.Count(), 1);

            Assert.That(tournament.Description == result.FirstOrDefault().Description);


        }


        [Test]
        public async Task EditTournament_ShouldBeTrue()
        {
            var model = new EditViewModel()
            {
                Id = 5392,
                Description = "changedDescriptionfortpurnament"
                
            };

            var tournament = new Tournament()
            {
                Id = 5392,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                CreatedOn = DateTime.Now,
                Description = "lkjhgfdvbjkiuytrdvbnjiuytfgbnjuytghjuytg",
                NumberOfTeams = 0,
                Status = FootballProjectSoftUni.Infrastructure.Data.Enums.TournamentStatus.Finished,
                OrganiserId = "600bafb9-a73d-4489-a387-643c3b8ae96c",
                RefereeId = null
            };

            _data.Tournaments.Add(tournament);
            _data.SaveChanges();

            await _tournamentService.EditTournamentAsync(model, DateTime.Now, DateTime.Now);

            Assert.AreEqual(tournament.Description, model.Description);
        }

        [Test]
        public async Task EditTournament_ShouldBeFalse()
        {
            EditViewModel model = new EditViewModel()
            {
                Id = -1
            };

            Assert.ThrowsAsync<ArgumentException>(() =>  _tournamentService.EditTournamentAsync(model, DateTime.Now, DateTime.Now));
        }

        [Test]

        public async Task DeleteTournament_ShouldReturnFirstFalse()
        {
            var result = await _tournamentService.DeleteTournamentAsync(9943);

            Assert.IsFalse(result);
        }

        [Test]

        public async Task DeleteTournament_ShouldReturnSecondFalse()
        {
            var tournament = new Tournament()
            {
                Id = 294,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                CreatedOn = DateTime.Now,
                Description = "lkjhgfdvbjkiuytrdvbnjiuytfgbnjuytghjuytg",
                NumberOfTeams = 0,
                Status = FootballProjectSoftUni.Infrastructure.Data.Enums.TournamentStatus.Finished,
                OrganiserId = "600bafb9-a73d-4489-a387-643c3b8ae96c",
                RefereeId = "trhgsvafc"
            };

            _data.Tournaments.Add(tournament);
            _data.SaveChanges();

            var result = await _tournamentService.DeleteTournamentAsync(tournament.Id);

            Assert.IsFalse(result);
        }

        [Test]

        public async Task DeleteTournament_ShouldReturnThirdFalse()
        {
            var tournament = new Tournament()
            {
                Id = 244,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                CreatedOn = DateTime.Now,
                Description = "lkjhgfdvbjkiuytrdvbnjiuytfgbnjuytghjuytg",
                NumberOfTeams = 0,
                Status = FootballProjectSoftUni.Infrastructure.Data.Enums.TournamentStatus.Finished,
                OrganiserId = "600bafb9-a73d-4489-a387-643c3b8ae96c",
                RefereeId = null
            };

            _data.Tournaments.Add(tournament);
            _data.SaveChanges();

            var referee = new Referee()
            {
                Id = "01bd2ve2-415q-4514-a6hb-11042df2acca",
                Birthdate = DateTime.Now.AddYears(-20),
                Name = "goshow",
                Experience = 5,
                Tournament = tournament,
                TournamentId = tournament.Id
            };

            _data.Referees.Add(referee);

            _data.SaveChanges();

            var result = await _tournamentService.DeleteTournamentAsync(tournament.Id);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task UpdateTournamentStatus_ShouldUpdateStatusCorrectly()
        {
            var tournament = new TournamentViewModel
            {
                StartDate = DateTime.Now.AddDays(-1),
                EndDate = DateTime.Now.AddDays(1)
            };

            MethodInfo method = typeof(TournamentService).GetMethod("UpdateTournamentStatus", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(_tournamentService, new object[] { tournament });

            Assert.AreEqual("Started", tournament.Status);
        }

        [Test]

        public async Task DeleteShould_WokrCorrectly()
        {
            var tournament = new Tournament()
            {
                Id = 24560,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                CreatedOn = DateTime.Now,
                Description = "lkjhgfdvbjkiuytrdvbnjiuytfgbnjuytghjuytg",
                NumberOfTeams = 0,
                Status = FootballProjectSoftUni.Infrastructure.Data.Enums.TournamentStatus.Finished,
                OrganiserId = "600bafb9-a73d-4489-a387-643c3b8ae96c",
                RefereeId = null
            };

            _data.Tournaments.Add(tournament);
            _data.SaveChanges();

            var tc = new TournamentCity()
            {
                TournamentId = tournament.Id,
                CityId = 2
            };

            _data.TournamentsCities.Add(tc);

            _data.SaveChanges();

            var referee = new Referee()
            {
                Id = "01bd2ve2-415q-45e4-a6hb-11042df2acca",
                Birthdate = DateTime.Now.AddYears(-20),
                Name = "goshow",
                Experience = 5,
                Tournament = tournament,
                TournamentId = tournament.Id
            };

            _data.Referees.Add(referee);

            _data.SaveChanges();

            var tp = new TournamentParticipant()
            {
                TournamentId = tournament.Id,
                ParticipantId = referee.Id,
                Role = "Referee"
            };

            _data.TournamentsParticipants.Add(tp);

            _data.SaveChanges();

            tournament.RefereeId = referee.Id;

            _data.SaveChanges();


            var result = await _tournamentService.DeleteTournamentAsync(tournament.Id);

            Assert.IsTrue(result);
        }

    }
}
