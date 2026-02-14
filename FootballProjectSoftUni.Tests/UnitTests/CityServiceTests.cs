using FootballProjectSoftUni.Core.Contracts.City;
using FootballProjectSoftUni.Core.Contracts.Coach;
using FootballProjectSoftUni.Core.Contracts.Referee;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Models.City;
using FootballProjectSoftUni.Core.Models.Coach;
using FootballProjectSoftUni.Core.Services.City;
using FootballProjectSoftUni.Core.Services.Tournament;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Stripe;
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
        private ITournamentService _tournamentService;

        [SetUp]

        public void SetUp()
        {
            _tournamentService = new TournamentService(_data);
            _cityService = new CityService(_data, _tournamentService);
        }

        [Test]
        public async Task FindTownAsync_ShouldReturnCorrectCity_WhenCityExists()
        {
            var expectedCityName = "Благоевград";
            var cityViewModel = new CityViewModel { Name = expectedCityName };

            var result = await _cityService.FindTownAsync(cityViewModel, 1);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedCityName, result.Name);
        }

        [Test]
        public async Task FindTownAsync_ShouldReturnNull_WhenCityDoesntExists()
        {
            CityViewModel cityViewModel = null;

            var result = await _cityService.FindTownAsync(cityViewModel, 30);

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
            var pageSize = 9; 
            var pageNumber = 2; 

            var citiesOnPage = await _cityService.AllCitiesAsync(pageNumber);

            var startIndex = (pageNumber - 1) * pageSize;
            var endIndex = startIndex + pageSize - 1;

            var expectedCities = await _data.Cities.OrderBy(c => c.Id)
                                                    .Skip(startIndex)
                                                    .Take(pageSize)
                                                    .ToListAsync();

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
        public async Task GetBestTeamsAsync_ShouldReturnTeamsOrderedByWinsDesc_AndMapCorrectly()
        {
            var city = new City { Id = 6543, Name = "Sofia", ImageUrl = "x" };
            _data.Cities.Add(city);

            var coach1 = new Coach { Id = "c1", Name = "Coach One", Experience = "3" };
            var coach2 = new Coach { Id = "c2", Name = "Coach Two", Experience = "5" };
            _data.Coaches.AddRange(coach1, coach2);

            var team1 = new Team { Id = 555, Name = "Team A", CoachId = coach1.Id, Coach = coach1, WinsCount = 0 };
            var team2 = new Team { Id = 5146511, Name = "Team B", CoachId = coach2.Id, Coach = coach2, WinsCount = 0 };
            _data.Teams.AddRange(team1, team2);

            _data.CityBestTeams.AddRange(
                new CityBestTeam { CityId = 6543, TeamId = 555, City = city, Team = team1, WinsInCity = 2 },
                new CityBestTeam { CityId = 6543, TeamId = 5146511, City = city, Team = team2, WinsInCity = 7 }
            );

            await _data.SaveChangesAsync();

            var result = await _cityService.GetBestTeamsAsync(6543);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));

            Assert.That(result[0].TeamId, Is.EqualTo(5146511));
            Assert.That(result[0].TeamName, Is.EqualTo("Team B"));
            Assert.That(result[0].CoachName, Is.EqualTo("Coach Two"));
            Assert.That(result[0].WinsInCity, Is.EqualTo(7));

            Assert.That(result[1].TeamId, Is.EqualTo(555));
            Assert.That(result[1].TeamName, Is.EqualTo("Team A"));
            Assert.That(result[1].CoachName, Is.EqualTo("Coach One"));
            Assert.That(result[1].WinsInCity, Is.EqualTo(2));
        }

        [Test]
        public async Task GetBestTeamsAsync_ShouldReturnEmptyList_WhenNoBestTeamsForCity()
        {
            _data.Cities.Add(new City { Id = 651, Name = "Plovdiv", ImageUrl = "x" });
            await _data.SaveChangesAsync();

            var result = await _cityService.GetBestTeamsAsync(2);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetUpdateCityBestTeamFormAsync_ShouldReturnCitiesAndTeams()
        {
            var city1 = new City { Id = 765, Name = "Sofia", ImageUrl = "img1" };
            var city2 = new City { Id = 259, Name = "Plovdiv", ImageUrl = "img2" };

            var coach = new Coach { Id = "coach156", Name = "Coach", Experience = "5" };

            var team1 = new Team { Id = 157, Name = "Team A", Coach = coach, CoachId = coach.Id };
            var team2 = new Team { Id = 297, Name = "Team B", Coach = coach, CoachId = coach.Id };

            _data.Cities.AddRange(city1, city2);
            _data.Coaches.Add(coach);
            _data.Teams.AddRange(team1, team2);

            await _data.SaveChangesAsync();

            var result = await _cityService.GetUpdateCityBestTeamFormAsync();

            Assert.That(result, Is.Not.Null);

            Assert.That(result.Cities.Count, Is.EqualTo(29));
            Assert.That(result.Teams.Count, Is.EqualTo(2));

            Assert.That(result.Cities.Any(c => c.Name == "Sofia"));
            Assert.That(result.Teams.Any(t => t.Name == "Team A"));
        }

        [Test]
        public async Task GetUpdateCityBestTeamFormAsync_ShouldMapCorrectIds()
        {
            var city = new City { Id = 342, Name = "Varna", ImageUrl = "x" };
            var coach = new Coach { Id = "thgrf", Name = "Coach", Experience = "2" };
            var team = new Team { Id = 7223, Name = "Champions", CoachId = coach.Id, Coach = coach };

            _data.Cities.Add(city);
            _data.Coaches.Add(coach);
            _data.Teams.Add(team);
            await _data.SaveChangesAsync();

            var result = await _cityService.GetUpdateCityBestTeamFormAsync();

            Assert.That(result.Cities.Any(c => c.Id == 342 && c.Name == "Varna"), Is.True);
            Assert.That(result.Teams.Any(t => t.Id == 7223 && t.Name == "Champions"), Is.True);
        }

        [Test]
        public async Task IncrementTeamWinsInCityAsync_ShouldCreateEntry_WhenNotExists()
        {
            int cityId = 1;
            int teamId = 10;

            await _cityService.IncrementTeamWinsInCityAsync(cityId, teamId);

            var entry = await _data.CityBestTeams
                .FirstOrDefaultAsync(x => x.CityId == cityId && x.TeamId == teamId);

            Assert.That(entry, Is.Not.Null);
            Assert.That(entry!.WinsInCity, Is.EqualTo(1));
        }

        [Test]
        public async Task IncrementTeamWinsInCityAsync_ShouldCreateEntryWithWins1_WhenEntryDoesNotExist()
        {
            var cityId = 9001;
            var teamId = 8001;

            var city = new City { Id = cityId, Name = "TestCity", ImageUrl = "x" };
            var coach = new Coach { Id = "coach-inc-1", Name = "Coach", Experience = "3" };
            var team = new Team { Id = teamId, Name = "TestTeam", CoachId = coach.Id, Coach = coach };

            _data.Cities.Add(city);
            _data.Coaches.Add(coach);
            _data.Teams.Add(team);
            await _data.SaveChangesAsync();

            await _cityService.IncrementTeamWinsInCityAsync(cityId, teamId);

            var entry = await _data.CityBestTeams
                .FirstOrDefaultAsync(cb => cb.CityId == cityId && cb.TeamId == teamId);

            Assert.That(entry, Is.Not.Null);
            Assert.That(entry!.WinsInCity, Is.EqualTo(1));
        }

        [Test]
        public async Task IncrementTeamWinsInCityAsync_ShouldIncrementWins_WhenEntryExists()
        {
            var cityId = 9002;
            var teamId = 8002;

            var city = new City { Id = cityId, Name = "TestCity2", ImageUrl = "x" };
            var coach = new Coach { Id = "coach-inc-2", Name = "Coach2", Experience = "4" };
            var team = new Team { Id = teamId, Name = "TestTeam2", CoachId = coach.Id, Coach = coach };

            _data.Cities.Add(city);
            _data.Coaches.Add(coach);
            _data.Teams.Add(team);

            _data.CityBestTeams.Add(new CityBestTeam
            {
                CityId = cityId,
                TeamId = teamId,
                WinsInCity = 2
            });

            await _data.SaveChangesAsync();

            await _cityService.IncrementTeamWinsInCityAsync(cityId, teamId);

            var entry = await _data.CityBestTeams
                .FirstOrDefaultAsync(cb => cb.CityId == cityId && cb.TeamId == teamId);

            Assert.That(entry, Is.Not.Null);
            Assert.That(entry!.WinsInCity, Is.EqualTo(3));
        }

        [Test]
        public async Task IncrementTeamWinsInCityAsync_ShouldIncreaseTo2_WhenCalledTwiceAndEntryInitiallyMissing()
        {
            var cityId = 9003;
            var teamId = 8003;

            var city = new City { Id = cityId, Name = "TestCity3", ImageUrl = "x" };
            var coach = new Coach { Id = "coach-inc-3", Name = "Coach3", Experience = "2" };
            var team = new Team { Id = teamId, Name = "TestTeam3", CoachId = coach.Id, Coach = coach };

            _data.Cities.Add(city);
            _data.Coaches.Add(coach);
            _data.Teams.Add(team);
            await _data.SaveChangesAsync();

            await _cityService.IncrementTeamWinsInCityAsync(cityId, teamId);
            await _cityService.IncrementTeamWinsInCityAsync(cityId, teamId);

            var entry = await _data.CityBestTeams
                .FirstOrDefaultAsync(cb => cb.CityId == cityId && cb.TeamId == teamId);

            Assert.That(entry, Is.Not.Null);
            Assert.That(entry!.WinsInCity, Is.EqualTo(2));
        }

    }
}
