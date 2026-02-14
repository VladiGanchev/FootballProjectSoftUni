using FootballProjectSoftUni.Core.Contracts.Email;
using FootballProjectSoftUni.Core.Contracts.Home;
using FootballProjectSoftUni.Core.Contracts.Notification;
using FootballProjectSoftUni.Core.Models.Email;
using FootballProjectSoftUni.Core.Services.Email;
using FootballProjectSoftUni.Core.Services.Home;
using FootballProjectSoftUni.Core.Services.Notification;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    public class HomeServiceTests : UnitTestsBase
    {
        private IHomeService homeService;

        [SetUp]
        public void SetUp()
        {
            homeService = new HomeService(_data);
        }

        [Test]
        public async Task GetHomePageData_ShouldReturnCountsFromAppStats()
        {
            var stats = await _data.AppStats.FindAsync(1);
            if (stats == null)
            {
                stats = new Infrastructure.Data.Models.TournamentStats
                {
                    Id = 1,
                    TournamentsCreatedTotal = 0,
                    PlayersCreatedTotal = 0,
                    TeamsCreatedTotal = 0
                };
                await _data.AppStats.AddAsync(stats);
                await _data.SaveChangesAsync();
            }

            stats.TournamentsCreatedTotal = 12;
            stats.PlayersCreatedTotal = 34;
            stats.TeamsCreatedTotal = 5;
            await _data.SaveChangesAsync();

            var service = new HomeService(_data);

            var result = await service.GetHomePageData();

            Assert.NotNull(result);
            Assert.That(result.PlayersCount, Is.EqualTo(34));
            Assert.That(result.TeamsCount, Is.EqualTo(5));
            Assert.That(result.TournamentsCount, Is.EqualTo(12));
            Assert.That(result.YearOfFoundation, Is.EqualTo("2025"));
        }

        [Test]
        public async Task GetHomePageData_ShouldThrow_WhenAppStatsMissing()
        {
            var stats = await _data.AppStats.FindAsync(1);
            if (stats != null)
            {
                _data.AppStats.Remove(stats);
                await _data.SaveChangesAsync();
            }

            var service = new HomeService(_data);

            Assert.ThrowsAsync<NullReferenceException>(async () => await service.GetHomePageData());
        }

    }
}
