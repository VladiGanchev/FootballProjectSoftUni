using FootballProjectSoftUni.Core.Contracts.City;
using FootballProjectSoftUni.Core.Contracts.Coach;
using FootballProjectSoftUni.Core.Contracts.Referee;
using FootballProjectSoftUni.Core.Models.City;
using FootballProjectSoftUni.Core.Models.Coach;
using FootballProjectSoftUni.Core.Services.City;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    public class CityServiceTests : UnitTestsBase
    {
        private ICityService _cityService;
        private IRefereeService _refereeService;
        private ICoachService _coachService;

        [OneTimeSetUp]

        public void SetUp() => _cityService = new CityService(_data);

        [Test]
        public async Task FindTownAsync_ShouldReturnCorrectCity_WhenCityExists()
        {
            // Arrange
            var expectedCityName = "Благоевград";
            var cityViewModel = new CityViewModel { Name = expectedCityName };

            // Act
            var result = await _cityService.FindTownAsync(cityViewModel, 1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedCityName, result.Name);
        }

        [Test]
        public async Task FindTownAsync_ShouldReturnNull_WhenCityDoesntExists()
        {
            // Arrange
            CityViewModel cityViewModel = null;

            // Act
            var result = await _cityService.FindTownAsync(cityViewModel, 30);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task AddCityAsyncShouldWorkCorrectly()
        {
            var citiesCountBefore = _data.Cities.Count();

            var city = new CityViewModel()
            {
                Name = "Велинград",
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/0/00/Velingrad_Areal_Image.jpg/1200px-Velingrad_Areal_Image.jpg"
            };


            await _cityService.AddCityAsync(city);

            var citiesCountAfter = _data.Cities.Count();

            Assert.AreEqual(citiesCountAfter, citiesCountBefore + 1);
        }

        [Test]

        public async Task AllCitiesAsyncShouldReturnAlTheCities()
        {
            var citiesCount = _data.Cities.Count();

            var citiesWithMethod = await _cityService.AllCitiesAsync();

            Assert.AreEqual(citiesWithMethod.Count(), citiesCount);
        }

        [Test]
        public async Task SearchAsyncShouldReturnMatchingCities()
        {
            var searchString = "Бур";

            var searchResults = await _cityService.SearchAsync(searchString);

            Assert.IsNotNull(searchResults);

            var expected = 1;

            var result = await _cityService.SearchAsync(searchString);

            Assert.AreEqual(expected, result.Count());
        }

        [Test]
        public async Task AllCitiesAsyncWithPageShouldReturnCorrectCities()
        {
            // Подгответе данни за тестване
            var pageSize = 8; // Брой градове на страница
            var pageNumber = 2; // Номер на страницата

            // Извикайте метода за получаване на градове с определена страница
            var citiesOnPage = await _cityService.AllCitiesAsync(pageNumber);

            // Пресметнете индексите на първия и последния град на страницата
            var startIndex = (pageNumber - 1) * pageSize;
            var endIndex = startIndex + pageSize - 1;

            // Извлечете градовете от базата данни, които се очакват да бъдат на страницата
            var expectedCities = await _data.Cities.OrderBy(c => c.Id)
                                                    .Skip(startIndex)
                                                    .Take(pageSize)
                                                    .ToListAsync();

            // Сравнете броя и данните на градовете, върнати от метода, с очакваните градове
            Assert.AreEqual(expectedCities.Count(), citiesOnPage.Count());
            Assert.IsTrue(expectedCities.Select(c => c.Name).SequenceEqual(citiesOnPage.Select(c => c.Name)));
            Assert.IsTrue(expectedCities.Select(c => c.ImageUrl).SequenceEqual(citiesOnPage.Select(c => c.ImageUrl)));
        }

        [Test]
        public async Task DeleteCityAsyncShouldDeleteCityAndRelatedEntities()
        {


            var cityIdToDelete = 27;

            var result = await _cityService.DeleteCityAsync(cityIdToDelete);

            Assert.IsTrue(result);

            // Проверете дали градът е изтрит от базата данни
            var deletedCity = await _data.Cities.FindAsync(cityIdToDelete);
            Assert.IsNull(deletedCity);

            Assert.IsFalse(await _cityService.DeleteCityAsync(30));


        }

        [Test]
        public async Task DeleteCity_ShouldReturnFalse_WhenIdIsNull()
        {
            int id = 30;
            CityViewModel model = null;

            var city = await _cityService.FindTownAsync(model, id);

            Assert.IsFalse(await _cityService.DeleteCityAsync(id));
        }

        [Test]

        public async Task DeleteCity_ShouldReturnNull_ForReferee()
        {
            Tournament tournament = new Tournament();

            _data.Tournaments.Add(tournament);

            _data.SaveChanges();

            var cityTournament = new TournamentCity()
            {
                CityId = 3,
                TournamentId = tournament.Id
            };

            _data.TournamentsCities.Add(cityTournament);

            _data.SaveChanges();

            var result = await _cityService.DeleteCityAsync(3);

            Assert.IsFalse(result);

        }

        [Test]

        public async Task DeleteCity_ShouldDeleteEverything_WhenEverithingIsCreated()
        {
            var tournament = new Tournament()
            {
                Id = 6,
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

            var cityTournament = new TournamentCity()
            {
                CityId = 5,
                TournamentId = tournament.Id
            };

            _data.TournamentsCities.Add(cityTournament);

            _data.SaveChanges();


            string id = "2424e1da-a66e-466e-bf7f-a59ecvf4b60b";
            var coach = new Coach()
            {
                Id = id,
                Name = "Pesho",
                Experience = "3"
            };

            _data.Coaches.Add(coach);

            _data.SaveChanges();

            var players = new List<Player>()
            {
                new Player()
                {
                    Id = 81,
                    Name = "1",
                    BirthDate = DateTime.Now,
                    TeamId = 0
                },
                new Player()
                {
                    Id = 82,
                    Name = "2",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 83,
                    Name = "3",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 84,
                    Name = "4",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 85,
                    Name = "5",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 86,
                    Name = "6",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 87,
                    Name = "7",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 88,
                    Name = "8",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 89,
                    Name = "9",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 90,
                    Name = "10",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
            };

            _data.Players.AddRange(players);

            _data.SaveChanges();

            var team = new Team()
            {
                Id = 3,
                Name = "qkite",
                CoachId = id,
                Coach = coach,
                Players = players,
            };

            _data.Teams.Add(team);

            _data.SaveChanges();


            foreach (var palyer in players)
            {
                palyer.TeamId = team.Id;
                palyer.Team = team;
            }

            _data.SaveChanges();

            team.CoachId = id;

            _data.SaveChanges();

            var tt = new TournamentTeam()
            {
                Team = team,
                TeamId = team.Id,
                TournamentId = tournament.Id
            };

            _data.TournamentsTeams.Add(tt);

            _data.SaveChanges();

            var tp = new TournamentParticipant()
            {
                ParticipantId = id,
                TournamentId = tournament.Id,
                Role = "Coach"

            };

            _data.TournamentsParticipants.Add(tp);

            _data.SaveChanges();

            var referee = new Referee()
            {
                Id = "c034dba6-00ae-45c4-b234-b9c72bc872ab",
                Birthdate = DateTime.Now.AddYears(-20),
                Name = "Misho",
                Experience = 7,
                TournamentId = 5,
                Tournament = tournament
            };

            _data.Referees.Add(referee);

            _data.SaveChanges();


            var tp2 = new TournamentParticipant()
            {
                ParticipantId = referee.Id,
                TournamentId = tournament.Id,
                Role = "Referee"

            };

            _data.TournamentsParticipants.Add(tp2);

            _data.SaveChanges();

            tournament.Referee = referee;
            tournament.RefereeId = referee.Id;

            _data.SaveChanges();

            await _cityService.DeleteCityAsync(5);

            Assert.IsNull(await _data.Tournaments.FindAsync(5));
            Assert.IsEmpty(await _data.TournamentsCities.Where(x => x.CityId == 4).ToListAsync());
            var deletedPlayers = await _data.Players.Where(p => p.TeamId == 2).ToListAsync();
            Assert.IsEmpty(deletedPlayers);
            var deletedTeam = await _data.Teams.FindAsync(2);
            Assert.IsNull(deletedTeam);
            var deletedTournamentTeam = await _data.TournamentsTeams.FirstOrDefaultAsync(tt => tt.TeamId == 2 && tt.TournamentId == 5);
            Assert.IsNull(deletedTournamentTeam);
            var deletedTournamentParticipant = await _data.TournamentsParticipants.Where(tp => tp.TournamentId == 5).ToListAsync();
            Assert.IsEmpty(deletedTournamentParticipant);
            var deletedReferee = await _data.Referees.FirstOrDefaultAsync(r => r.Id == "c034dba6-00ae-45c4-b234-b9c72bc872ab");
            Assert.IsNull(deletedReferee);

        }


        
    }
}
