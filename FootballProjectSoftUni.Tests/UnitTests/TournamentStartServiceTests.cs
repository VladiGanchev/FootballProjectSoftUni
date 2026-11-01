using FootballProjectSoftUni.Core.Models.Tournament;
using FootballProjectSoftUni.Core.Services.City;
using FootballProjectSoftUni.Core.Services.Coach;
using FootballProjectSoftUni.Core.Services.Tournament;
using FootballProjectSoftUni.Infrastructure.Data.Enums;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using FootballProjectSoftUni.Tests.Mocks;
using Microsoft.EntityFrameworkCore;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    public class TournamentStartServiceTests : UnitTestsBase
    {
        private const int CityId = 22; // София (seeded)

        private TournamentService _tournaments = null!;
        private FakePaymentService _payments = null!;
        private TournamentStartService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _tournaments = new TournamentService(_data);
            _payments = new FakePaymentService(_data);

            var deletion = new TournamentDeletionService(
                _data, _payments, new CoachService(_data, _tournaments), _tournaments, new CityService(_data, _tournaments));

            _service = new TournamentStartService(_data, _tournaments, deletion);
        }

        // ---------- bracket ----------

        [Test]
        public async Task AssignTeam_FillsOnePlaceInEveryMatchFirst()
        {
            await AddTournamentAsync(7001, DateTime.Now.AddDays(3));
            await AddTeamsAsync(7001, count: 10);

            var round1 = await FirstRoundAsync(7001);

            // Teams 1-8 open the 8 matches, teams 9 and 10 become the opponents of teams 1 and 2.
            Assert.That(round1.Select(m => m.Team1Id), Is.EqualTo(Enumerable.Range(1, 8).Select(i => (int?)(7100 + i))));
            Assert.That(round1[0].Team2Id, Is.EqualTo(7109));
            Assert.That(round1[1].Team2Id, Is.EqualTo(7110));
            Assert.That(round1.Skip(2).All(m => m.Team2Id == null), Is.True);
        }

        [Test]
        public async Task PrepareForStart_TeamsWithoutOpponentGoThrough()
        {
            await AddTournamentAsync(7002, DateTime.Now.AddDays(3));
            await AddTeamsAsync(7002, count: 10);

            await _tournaments.PrepareBracketForStartAsync(7002);

            var round1 = await FirstRoundAsync(7002);
            Assert.That(round1.Take(2).All(m => m.WinnerTeamId == null), Is.True, "Real matches are still to be played.");
            Assert.That(round1.Skip(2).All(m => m.WinnerTeamId == m.Team1Id), Is.True);

            var round2 = await RoundAsync(7002, 2);
            Assert.That(round2[1].Team1Id, Is.EqualTo(7103));
            Assert.That(round2[1].Team2Id, Is.EqualTo(7104));
            Assert.That(round2[3].Team1Id, Is.EqualTo(7107));
            Assert.That(round2[3].Team2Id, Is.EqualTo(7108));
            Assert.That(round2[0].Team1Id, Is.Null, "Waits for the winners of matches 1 and 2.");
        }

        [Test]
        public async Task PrepareForStart_SpreadsBracketFilledTheOldWay()
        {
            await AddTournamentAsync(7003, DateTime.Now.AddDays(3));
            await _tournaments.GenerateBracketAsync(7003);

            // Old order: 1 v 2, 3 v 4 ... - the last three matches have no team at all.
            var round1 = await _data.Matches.Where(m => m.TournamentId == 7003 && m.Round == 1).OrderBy(m => m.IndexInRound).ToListAsync();
            for (var i = 0; i < 10; i++)
            {
                if (i % 2 == 0) round1[i / 2].Team1Id = 7301 + i;
                else round1[i / 2].Team2Id = 7301 + i;
            }
            await _data.SaveChangesAsync();

            await _tournaments.PrepareBracketForStartAsync(7003);

            var after = await FirstRoundAsync(7003);
            Assert.That(after.All(m => m.Team1Id.HasValue), Is.True, "Every first-round match has a team now.");
            Assert.That(after.SelectMany(m => new[] { m.Team1Id, m.Team2Id }).Count(id => id.HasValue), Is.EqualTo(10));
            Assert.That(after.Count(m => m.WinnerTeamId.HasValue), Is.EqualTo(6), "Six teams go through without a match.");
        }

        // ---------- start ----------

        [Test]
        public async Task TooFewTeams_AtStart_TournamentIsCancelledAndPaidTeamsRefunded()
        {
            await AddTournamentAsync(7004, DateTime.Now.AddMinutes(-2));
            await AddTeamsAsync(7004, count: 5);
            await AddPaidOrderAsync(7004, orderId: 7401, teamId: 7101);
            await AddPaidOrderAsync(7004, orderId: 7402, teamId: 7102);

            var results = await _service.StartDueTournamentsAsync();

            var result = results.Single();
            Assert.That(result.Outcome, Is.EqualTo(TournamentStartOutcome.Canceled));
            Assert.That(result.Teams, Is.EqualTo(5));
            Assert.That(_payments.RefundedAfterStart, Is.EquivalentTo(new[] { 7401, 7402 }), "Refunds are allowed although the start time has passed.");
            Assert.IsFalse(await _data.Tournaments.AnyAsync(t => t.Id == 7004));
            Assert.That(await _data.TournamentJoinPayments.CountAsync(p => p.Id == 7401 || p.Id == 7402), Is.EqualTo(2), "Payments are kept.");
        }

        [Test]
        public async Task EnoughTeams_AtStart_TournamentStarts()
        {
            await AddTournamentAsync(7005, DateTime.Now.AddMinutes(-2));
            await AddTeamsAsync(7005, count: 9);

            var result = (await _service.StartDueTournamentsAsync()).Single();

            Assert.That(result.Outcome, Is.EqualTo(TournamentStartOutcome.Started));
            Assert.That((await _data.Tournaments.AsNoTracking().FirstAsync(t => t.Id == 7005)).Status, Is.EqualTo(TournamentStatus.Started));
            Assert.That((await FirstRoundAsync(7005)).Count(m => m.WinnerTeamId.HasValue), Is.EqualTo(7));

            Assert.IsEmpty(await _service.StartDueTournamentsAsync(), "A started tournament is handled only once.");
        }

        [Test]
        public async Task WhenRefundFails_NothingIsDeleted_AndItIsTriedAgain()
        {
            await AddTournamentAsync(7006, DateTime.Now.AddMinutes(-2));
            await AddTeamsAsync(7006, count: 3);
            await AddPaidOrderAsync(7006, orderId: 7601, teamId: 7101);
            _payments.FailingOrders.Add(7601);

            var result = (await _service.StartDueTournamentsAsync()).Single();

            Assert.That(result.Outcome, Is.EqualTo(TournamentStartOutcome.CancelFailed));
            Assert.IsTrue(await _data.Tournaments.AnyAsync(t => t.Id == 7006));

            _payments.FailingOrders.Clear();
            var retry = (await _service.StartDueTournamentsAsync()).Single();
            Assert.That(retry.Outcome, Is.EqualTo(TournamentStartOutcome.Canceled));
        }

        [Test]
        public async Task TournamentsThatStartedLongAgo_AreNotTouched()
        {
            // Safety net: whatever its status, a tournament is never cancelled long after its start.
            await AddTournamentAsync(7007, DateTime.Now.AddDays(-10));
            await AddTeamsAsync(7007, count: 3);

            Assert.IsEmpty(await _service.StartDueTournamentsAsync());
            Assert.IsTrue(await _data.Tournaments.AnyAsync(t => t.Id == 7007));
        }

        // ---------- warning 24 h before ----------

        [Test]
        public async Task LowTeamsWarning_IsGivenOnce_OnlyForTournamentsWithTooFewTeams()
        {
            await AddTournamentAsync(7008, DateTime.Now.AddHours(20));
            await AddTeamsAsync(7008, count: 5);
            await AddTournamentAsync(7009, DateTime.Now.AddHours(20));
            await AddTeamsAsync(7009, count: 8, firstTeamId: 7201);
            await AddTournamentAsync(7010, DateTime.Now.AddDays(3));
            await AddTeamsAsync(7010, count: 2, firstTeamId: 7301);

            var warnings = await _service.TakeLowTeamsWarningsAsync();

            Assert.That(warnings.Select(w => w.TournamentId), Is.EquivalentTo(new[] { 7008 }));
            Assert.That(warnings[0].Teams, Is.EqualTo(5));
            Assert.That(warnings[0].City, Is.EqualTo("София"));
            Assert.IsEmpty(await _service.TakeLowTeamsWarningsAsync(), "Each tournament is reported only once.");
        }

        private async Task AddTournamentAsync(int id, DateTime start)
        {
            await _data.Tournaments.AddAsync(new Tournament
            {
                Id = id,
                StartDate = start,
                EndDate = start.AddHours(8),
                Description = $"Tournament {id}",
                OrganiserId = "org-1",
                CreatedOn = DateTime.Now,
                ImageUrl = "https://example.com/t.jpg",
                Status = TournamentStatus.Upcoming,
                ParticipationFee = 10m
            });
            await _data.TournamentsCities.AddAsync(new TournamentCity { TournamentId = id, CityId = CityId });
            await _data.SaveChangesAsync();
        }

        /// <summary>Registers teams the way joining does: tournament link + a place in the bracket.</summary>
        private async Task AddTeamsAsync(int tournamentId, int count, int firstTeamId = 7101)
        {
            await _tournaments.GenerateBracketAsync(tournamentId);

            for (var i = 0; i < count; i++)
            {
                var teamId = firstTeamId + i;
                if (!await _data.Teams.AnyAsync(t => t.Id == teamId))
                {
                    await _data.Teams.AddAsync(new Team { Id = teamId, Name = $"Team {teamId}", CoachId = $"coach-{teamId}" });
                }

                await _data.TournamentsTeams.AddAsync(new TournamentTeam { TournamentId = tournamentId, TeamId = teamId });
                await _data.SaveChangesAsync();

                await _tournaments.AssignTeamToBracketAsync(tournamentId, teamId);
            }
        }

        private async Task AddPaidOrderAsync(int tournamentId, int orderId, int teamId)
        {
            await _data.TournamentJoinPayments.AddAsync(new TournamentJoinPayment
            {
                Id = orderId,
                TournamentId = tournamentId,
                UserId = $"coach-{teamId}",
                TeamId = teamId,
                Amount = 10m,
                Currency = "eur",
                Status = "Paid"
            });
            await _data.SaveChangesAsync();
        }

        private Task<List<Match>> FirstRoundAsync(int tournamentId) => RoundAsync(tournamentId, 1);

        private Task<List<Match>> RoundAsync(int tournamentId, int round) =>
            _data.Matches.AsNoTracking()
                .Where(m => m.TournamentId == tournamentId && m.Round == round)
                .OrderBy(m => m.IndexInRound)
                .ToListAsync();
    }
}
