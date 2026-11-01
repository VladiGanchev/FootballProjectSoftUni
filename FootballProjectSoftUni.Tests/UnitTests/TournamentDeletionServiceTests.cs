using FootballProjectSoftUni.Core.Contracts.Payment;
using FootballProjectSoftUni.Core.Models.AdminPayments;
using FootballProjectSoftUni.Core.Models.Payment;
using FootballProjectSoftUni.Core.Models.Tournament;
using FootballProjectSoftUni.Core.Services.City;
using FootballProjectSoftUni.Core.Services.Coach;
using FootballProjectSoftUni.Core.Services.Tournament;
using FootballProjectSoftUni.Infrastructure.Data;
using FootballProjectSoftUni.Infrastructure.Data.Enums;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using FootballProjectSoftUni.Tests.Mocks;
using Microsoft.EntityFrameworkCore;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    public class TournamentDeletionServiceTests : UnitTestsBase
    {
        private const int CityId = 22; // София (seeded)

        private FakePaymentService _payments = null!;
        private TournamentDeletionService _service = null!;

        [SetUp]
        public void SetUp()
        {
            var tournamentService = new TournamentService(_data);
            _payments = new FakePaymentService(_data);

            _service = new TournamentDeletionService(
                _data,
                _payments,
                new CoachService(_data, tournamentService),
                tournamentService,
                new CityService(_data, tournamentService));
        }

        [Test]
        public async Task NotStarted_WithPaidTeams_RefundsEveryoneThenDeletes()
        {
            await AddTournamentAsync(9001, DateTime.Now.AddDays(5), DateTime.Now.AddDays(6));
            await AddPaidTeamAsync(9001, orderId: 901, teamId: 9101, userId: "coach-1");
            await AddPaidTeamAsync(9001, orderId: 902, teamId: 9102, userId: "coach-2");

            var result = await _service.DeleteTournamentAsync(9001);

            Assert.That(result.Status, Is.EqualTo(TournamentDeletionStatus.Deleted));
            Assert.That(_payments.RefundedOrders, Is.EquivalentTo(new[] { 901, 902 }));
            Assert.That(_payments.ExpiredTournaments, Does.Contain(9001));
            Assert.That(result.Refunded.Select(r => r.UserId), Is.EquivalentTo(new[] { "coach-1", "coach-2" }));
            Assert.IsFalse(await _data.Tournaments.AnyAsync(t => t.Id == 9001));

            // The payments are kept as records, with what they were for.
            var payments = await _data.TournamentJoinPayments.AsNoTracking().Where(p => p.Id == 901 || p.Id == 902).ToListAsync();
            Assert.That(payments.Count, Is.EqualTo(2));
            Assert.That(payments.All(p => p.Status == "Refunded" && p.TournamentId == null), Is.True);
            Assert.That(payments.All(p => p.TournamentCity == "София" && p.TournamentStartDate != null), Is.True);

            // Its picture is removed from the site afterwards.
            Assert.That(result.DeletedPictureUrls, Is.EquivalentTo(new[] { "https://example.com/t.jpg" }));
        }

        [Test]
        public async Task Deleted_ReportsThePictureOfTheTournament()
        {
            await AddTournamentAsync(9010, DateTime.Now.AddDays(-3), DateTime.Now.AddDays(-1), image: "/images/covers/tournaments/t9010.jpg");

            var result = await _service.DeleteTournamentAsync(9010);

            Assert.That(result.Status, Is.EqualTo(TournamentDeletionStatus.Deleted));
            Assert.That(result.DeletedPictureUrls, Is.EquivalentTo(new[] { "/images/covers/tournaments/t9010.jpg" }));
        }

        [Test]
        public async Task NotStarted_WhenARefundFails_NothingIsDeleted_AndRefundedTeamIsOut()
        {
            await AddTournamentAsync(9002, DateTime.Now.AddDays(5), DateTime.Now.AddDays(6));
            await AddPaidTeamAsync(9002, orderId: 911, teamId: 9111, userId: "coach-ok");
            await AddPaidTeamAsync(9002, orderId: 912, teamId: 9112, userId: "coach-fail");
            _payments.FailingOrders.Add(912);

            var result = await _service.DeleteTournamentAsync(9002);

            Assert.That(result.Status, Is.EqualTo(TournamentDeletionStatus.RefundsFailed));
            Assert.That(result.FailedTeams, Is.EquivalentTo(new[] { "Team 9112" }));
            Assert.That(result.Refunded.Select(r => r.UserId), Is.EquivalentTo(new[] { "coach-ok" }));
            Assert.IsTrue(await _data.Tournaments.AnyAsync(t => t.Id == 9002));

            // The refunded team is out, the other one still plays and its order is still paid.
            Assert.IsFalse(await _data.TournamentsTeams.AnyAsync(tt => tt.TournamentId == 9002 && tt.TeamId == 9111));
            Assert.IsTrue(await _data.TournamentsTeams.AnyAsync(tt => tt.TournamentId == 9002 && tt.TeamId == 9112));
            Assert.That((await _data.TournamentJoinPayments.AsNoTracking().FirstAsync(p => p.Id == 912)).Status, Is.EqualTo("Paid"));
            Assert.IsEmpty(result.DeletedTournamentIds);
            Assert.IsEmpty(result.DeletedPictureUrls);
        }

        [Test]
        public async Task Running_CannotBeDeleted()
        {
            await AddTournamentAsync(9003, DateTime.Now.AddHours(-1), DateTime.Now.AddDays(1));
            await AddPaidTeamAsync(9003, orderId: 921, teamId: 9121, userId: "coach-run");

            var result = await _service.DeleteTournamentAsync(9003);

            Assert.That(result.Status, Is.EqualTo(TournamentDeletionStatus.Running));
            Assert.IsNotNull(result.RunningUntil);
            Assert.IsEmpty(_payments.RefundedOrders);
            Assert.IsTrue(await _data.Tournaments.AnyAsync(t => t.Id == 9003));
        }

        [Test]
        public async Task Finished_IsDeletedWithoutRefunds_AndPaymentIsKept()
        {
            await AddTournamentAsync(9004, DateTime.Now.AddDays(-3), DateTime.Now.AddDays(-1));
            await AddPaidTeamAsync(9004, orderId: 931, teamId: 9131, userId: "coach-old");

            var result = await _service.DeleteTournamentAsync(9004);

            Assert.That(result.Status, Is.EqualTo(TournamentDeletionStatus.Deleted));
            Assert.IsEmpty(_payments.RefundedOrders);
            Assert.IsEmpty(result.Refunded);

            var payment = await _data.TournamentJoinPayments.AsNoTracking().FirstAsync(p => p.Id == 931);
            Assert.That(payment.Status, Is.EqualTo("Paid"));
            Assert.IsNull(payment.TournamentId);
            Assert.That(payment.TournamentCity, Is.EqualTo("София"));
        }

        [Test]
        public async Task Preview_NotStarted_CountsOnlyPaidTeams()
        {
            await AddTournamentAsync(9005, DateTime.Now.AddDays(5), DateTime.Now.AddDays(6));
            await AddPaidTeamAsync(9005, orderId: 941, teamId: 9141, userId: "coach-a", amount: 10m);
            await AddPaidTeamAsync(9005, orderId: 942, teamId: 9142, userId: "coach-b", amount: 15m);
            await AddPaidTeamAsync(9005, orderId: 943, teamId: 9143, userId: "coach-c", status: "Pending");

            var preview = await _service.PreviewTournamentAsync(9005);

            Assert.IsNotNull(preview);
            Assert.That(preview!.PaidTeams, Is.EqualTo(2));
            Assert.That(preview.PaidTotal, Is.EqualTo(25m));
            Assert.IsFalse(preview.IsRunning);
            Assert.IsFalse(preview.IsFinished);
        }

        [Test]
        public async Task NotStarted_OtherParticipantsAreReported()
        {
            await AddTournamentAsync(9006, DateTime.Now.AddDays(5), DateTime.Now.AddDays(6));
            await AddPaidTeamAsync(9006, orderId: 951, teamId: 9151, userId: "coach-paid");
            await _data.TournamentsParticipants.AddAsync(new TournamentParticipant { ParticipantId = "referee-1", TournamentId = 9006, Role = "Referee" });
            await _data.SaveChangesAsync();

            var result = await _service.DeleteTournamentAsync(9006);

            Assert.That(result.Status, Is.EqualTo(TournamentDeletionStatus.Deleted));
            Assert.That(result.OtherParticipants.Select(p => p.UserId), Is.EquivalentTo(new[] { "referee-1" }));
        }

        [Test]
        public async Task City_WithRunningTournament_CannotBeDeleted()
        {
            await AddTournamentAsync(9007, DateTime.Now.AddHours(-1), DateTime.Now.AddDays(1));

            var result = await _service.DeleteCityAsync(CityId);

            Assert.That(result.Status, Is.EqualTo(TournamentDeletionStatus.Running));
            Assert.IsTrue(await _data.Cities.AnyAsync(c => c.Id == CityId));
            Assert.IsTrue(await _data.Tournaments.AnyAsync(t => t.Id == 9007));
            Assert.IsEmpty(result.DeletedPictureUrls);
        }

        [Test]
        public async Task City_IsDeletedWithTournamentsAndRanking_TeamsAndPaymentsStay()
        {
            await AddTournamentAsync(9008, DateTime.Now.AddDays(-3), DateTime.Now.AddDays(-1), image: "/images/covers/tournaments/t9008.jpg");
            await AddPaidTeamAsync(9008, orderId: 961, teamId: 9161, userId: "coach-x");
            await AddTournamentAsync(9009, DateTime.Now.AddDays(5), DateTime.Now.AddDays(6), image: "/images/covers/tournaments/t9009.jpg");
            await AddPaidTeamAsync(9009, orderId: 962, teamId: 9162, userId: "coach-y");
            await _data.CityBestTeams.AddAsync(new CityBestTeam { CityId = CityId, TeamId = 9161, WinsInCity = 2 });
            await _data.SaveChangesAsync();
            var cityPicture = (await _data.Cities.AsNoTracking().FirstAsync(c => c.Id == CityId)).ImageUrl;

            var result = await _service.DeleteCityAsync(CityId);

            Assert.That(result.Status, Is.EqualTo(TournamentDeletionStatus.Deleted));
            Assert.IsFalse(await _data.Cities.AnyAsync(c => c.Id == CityId));
            Assert.IsFalse(await _data.Tournaments.AnyAsync(t => t.Id == 9008 || t.Id == 9009));
            Assert.IsFalse(await _data.CityBestTeams.AnyAsync(b => b.CityId == CityId));
            Assert.That(result.DeletedTournamentIds, Is.EquivalentTo(new[] { 9008, 9009 }));
            Assert.That(result.DeletedPictureUrls, Is.EquivalentTo(new[] { cityPicture, "/images/covers/tournaments/t9008.jpg", "/images/covers/tournaments/t9009.jpg" }));

            // Only the not-started tournament is refunded; teams and payments stay.
            Assert.That(_payments.RefundedOrders, Is.EquivalentTo(new[] { 962 }));
            Assert.IsTrue(await _data.Teams.AnyAsync(t => t.Id == 9161));
            Assert.That(await _data.TournamentJoinPayments.CountAsync(p => p.Id == 961 || p.Id == 962), Is.EqualTo(2));
        }

        private async Task AddTournamentAsync(int id, DateTime start, DateTime end, string image = "https://example.com/t.jpg")
        {
            await _data.Tournaments.AddAsync(new Tournament
            {
                Id = id,
                StartDate = start,
                EndDate = end,
                Description = $"Tournament {id}",
                OrganiserId = "org-1",
                NumberOfTeams = 0,
                CreatedOn = DateTime.Now,
                ImageUrl = image,
                Status = TournamentStatus.Upcoming,
                ParticipationFee = 10m
            });
            await _data.TournamentsCities.AddAsync(new TournamentCity { TournamentId = id, CityId = CityId });
            await _data.SaveChangesAsync();
        }

        private async Task AddPaidTeamAsync(int tournamentId, int orderId, int teamId, string userId, decimal amount = 10m, string status = "Paid")
        {
            await _data.Teams.AddAsync(new Team { Id = teamId, Name = $"Team {teamId}", CoachId = userId });
            await _data.Coaches.AddAsync(new Coach { Id = userId, Name = userId, Experience = "3", TeamId = teamId });
            await _data.TournamentsTeams.AddAsync(new TournamentTeam { TournamentId = tournamentId, TeamId = teamId });
            await _data.TournamentsParticipants.AddAsync(new TournamentParticipant { ParticipantId = userId, TournamentId = tournamentId, Role = "Coach" });
            await _data.TournamentJoinPayments.AddAsync(new TournamentJoinPayment
            {
                Id = orderId,
                TournamentId = tournamentId,
                UserId = userId,
                TeamId = teamId,
                Amount = amount,
                Currency = "eur",
                Status = status
            });
            await _data.SaveChangesAsync();
        }
    }
}
