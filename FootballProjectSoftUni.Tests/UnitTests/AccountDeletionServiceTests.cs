using FootballProjectSoftUni.Core.Models.Profile;
using FootballProjectSoftUni.Core.Services.Profile;
using FootballProjectSoftUni.Infrastructure.Data.Enums;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static FootballProjectSoftUni.Core.Constants.RoleConstants;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    public class AccountDeletionServiceTests : UnitTestsBase
    {
        private const int CityId = 22; // София (seeded)

        private AccountDeletionService _service = null!;

        [SetUp]
        public void SetUp() => _service = new AccountDeletionService(_data);

        [Test]
        public async Task Admin_CannotDeleteAccount()
        {
            await AddUserAsync("admin-1");
            await _data.Roles.AddAsync(new IdentityRole { Id = "role-admin", Name = AdminRole, NormalizedName = AdminRole.ToUpperInvariant() });
            await _data.UserRoles.AddAsync(new IdentityUserRole<string> { UserId = "admin-1", RoleId = "role-admin" });
            await _data.SaveChangesAsync();

            var result = await _service.DeleteAccountAsync("admin-1");

            Assert.That(result.Block, Is.EqualTo(AccountDeletionBlock.Organizer));
            Assert.IsTrue(await _data.Users.AnyAsync(u => u.Id == "admin-1"));
        }

        [Test]
        public async Task Organizer_CannotDeleteAccount()
        {
            await AddUserAsync("org-1");
            await AddTournamentAsync(8001, DateTime.Now.AddDays(-5), DateTime.Now.AddDays(-4), organiserId: "org-1");

            var result = await _service.CheckAsync("org-1");

            Assert.That(result.Block, Is.EqualTo(AccountDeletionBlock.Organizer));
        }

        [Test]
        public async Task Coach_InUpcomingTournament_IsBlocked()
        {
            await AddCoachWithTeamAsync("coach-1", teamId: 8101);
            await AddTournamentAsync(8002, DateTime.Now.AddDays(3), DateTime.Now.AddDays(4));
            await _data.TournamentsTeams.AddAsync(new TournamentTeam { TeamId = 8101, TournamentId = 8002 });
            await _data.SaveChangesAsync();

            var result = await _service.DeleteAccountAsync("coach-1");

            Assert.That(result.Block, Is.EqualTo(AccountDeletionBlock.CoachInTournament));
            Assert.IsFalse(result.TournamentStarted);
            Assert.That(result.TournamentCity, Is.EqualTo("София"));
            Assert.IsTrue(await _data.Users.AnyAsync(u => u.Id == "coach-1"));
        }

        [Test]
        public async Task Coach_InRunningTournament_IsBlockedUntilTheEnd()
        {
            await AddCoachWithTeamAsync("coach-2", teamId: 8102);
            await AddTournamentAsync(8003, DateTime.Now.AddHours(-1), DateTime.Now.AddDays(1));
            await _data.TournamentsTeams.AddAsync(new TournamentTeam { TeamId = 8102, TournamentId = 8003 });
            await _data.SaveChangesAsync();

            var result = await _service.CheckAsync("coach-2");

            Assert.That(result.Block, Is.EqualTo(AccountDeletionBlock.CoachInTournament));
            Assert.IsTrue(result.TournamentStarted);
        }

        [Test]
        public async Task Coach_WhoPlayedBefore_TeamNameStays_PlayersAndAccountGo_PaymentsStay()
        {
            await AddCoachWithTeamAsync("coach-3", teamId: 8103);
            await AddTournamentAsync(8004, DateTime.Now.AddDays(-5), DateTime.Now.AddDays(-4));
            await _data.TournamentsTeams.AddAsync(new TournamentTeam { TeamId = 8103, TournamentId = 8004 });
            await _data.TournamentsParticipants.AddAsync(new TournamentParticipant { ParticipantId = "coach-3", TournamentId = 8004, Role = "Coach" });
            await _data.TournamentJoinPayments.AddAsync(new TournamentJoinPayment { Id = 8201, UserId = "coach-3", TournamentId = 8004, TeamId = 8103, Amount = 10m, Currency = "eur", Status = "Paid" });
            await _data.SaveChangesAsync();

            var result = await _service.DeleteAccountAsync("coach-3");

            Assert.IsTrue(result.CanDelete);
            Assert.IsFalse(await _data.Users.AnyAsync(u => u.Id == "coach-3"));
            Assert.IsFalse(await _data.Coaches.AnyAsync(c => c.Id == "coach-3"));
            Assert.IsFalse(await _data.Players.AnyAsync(p => p.TeamId == 8103));
            Assert.IsFalse(await _data.TournamentsParticipants.AnyAsync(tp => tp.ParticipantId == "coach-3"));

            var team = await _data.Teams.AsNoTracking().FirstOrDefaultAsync(t => t.Id == 8103);
            Assert.IsNotNull(team, "The team stays in the tournament history.");
            Assert.That(team!.CoachId, Is.Empty);

            Assert.IsTrue(await _data.TournamentJoinPayments.AnyAsync(p => p.Id == 8201), "Payments are accounting records.");
        }

        [Test]
        public async Task Coach_WhoNeverPlayed_TeamIsDeletedToo()
        {
            await AddCoachWithTeamAsync("coach-4", teamId: 8104);

            var result = await _service.DeleteAccountAsync("coach-4");

            Assert.IsTrue(result.CanDelete);
            Assert.IsFalse(await _data.Teams.AnyAsync(t => t.Id == 8104));
            Assert.IsFalse(await _data.Players.AnyAsync(p => p.TeamId == 8104));
        }

        [Test]
        public async Task Referee_OfUpcomingTournament_IsBlocked()
        {
            await AddUserAsync("ref-1");
            await AddTournamentAsync(8005, DateTime.Now.AddDays(3), DateTime.Now.AddDays(4), refereeId: "ref-1");
            await _data.Referees.AddAsync(new Referee { Id = "ref-1", Name = "Ref", Birthdate = new DateTime(1990, 1, 1), Experience = 5, TournamentId = 8005 });
            await _data.SaveChangesAsync();

            var result = await _service.DeleteAccountAsync("ref-1");

            Assert.That(result.Block, Is.EqualTo(AccountDeletionBlock.RefereeInTournament));
            Assert.IsTrue(await _data.Referees.AnyAsync(r => r.Id == "ref-1"));
        }

        [Test]
        public async Task Referee_AfterFinishedTournament_ProfileRatingsAndCommentsGo()
        {
            await AddUserAsync("ref-2");
            await AddTournamentAsync(8006, DateTime.Now.AddDays(-5), DateTime.Now.AddDays(-4), refereeId: "ref-2");
            await _data.Referees.AddAsync(new Referee { Id = "ref-2", Name = "Ref", Birthdate = new DateTime(1990, 1, 1), Experience = 5, TournamentId = 8006 });
            await _data.RefereesRatings.AddAsync(new RefereeRating { RefereeId = "ref-2", UserId = "someone", Value = 4 });
            await _data.RefereeComments.AddAsync(new RefereeComment { RefereeId = "ref-2", UserId = "someone", Content = "Good" });
            await _data.SaveChangesAsync();

            var result = await _service.DeleteAccountAsync("ref-2");

            Assert.IsTrue(result.CanDelete);
            Assert.IsFalse(await _data.Referees.AnyAsync(r => r.Id == "ref-2"));
            Assert.IsFalse(await _data.RefereesRatings.AnyAsync(r => r.RefereeId == "ref-2"));
            Assert.IsFalse(await _data.RefereeComments.AnyAsync(c => c.RefereeId == "ref-2"));
            Assert.IsNull((await _data.Tournaments.AsNoTracking().FirstAsync(t => t.Id == 8006)).RefereeId);
        }

        [Test]
        public async Task ConversationsRatingsAndCommentsWrittenByTheUser_AreDeleted()
        {
            await AddUserAsync("user-1");
            await AddUserAsync("admin-x");
            await _data.Referees.AddAsync(new Referee { Id = "other-ref", Name = "Other", Birthdate = new DateTime(1990, 1, 1), Experience = 5 });

            var question = new ContactMessage { Id = 8301, UserId = "user-1", Subject = "Q", Content = "Hello", IsFromAdmin = false };
            var answer = new ContactMessage { Id = 8302, UserId = "user-1", ParentMessageId = 8301, Subject = "Q", Content = "Hi", IsFromAdmin = true };
            await _data.ContactMessages.AddRangeAsync(question, answer);
            await _data.Notifications.AddRangeAsync(
                new Notification { UserId = "admin-x", ContactMessageId = 8301, Message = "Q" },
                new Notification { UserId = "user-1", ContactMessageId = 8302, Message = "Q" },
                new Notification { UserId = "admin-x", Message = "unrelated" });
            await _data.RefereesRatings.AddAsync(new RefereeRating { RefereeId = "other-ref", UserId = "user-1", Value = 2 });
            await _data.RefereeComments.AddAsync(new RefereeComment { RefereeId = "other-ref", UserId = "user-1", Content = "Bad" });
            await _data.SaveChangesAsync();

            var result = await _service.DeleteAccountAsync("user-1");

            Assert.IsTrue(result.CanDelete);
            Assert.IsFalse(await _data.ContactMessages.AnyAsync(m => m.UserId == "user-1"));
            Assert.IsFalse(await _data.Notifications.AnyAsync(n => n.ContactMessageId == 8301 || n.ContactMessageId == 8302));
            Assert.IsTrue(await _data.Notifications.AnyAsync(n => n.Message == "unrelated"));
            Assert.IsFalse(await _data.RefereesRatings.AnyAsync(r => r.UserId == "user-1"));
            Assert.IsFalse(await _data.RefereeComments.AnyAsync(c => c.UserId == "user-1"));
            Assert.IsTrue(await _data.Referees.AnyAsync(r => r.Id == "other-ref"), "Other referees are not touched.");
        }

        private async Task AddUserAsync(string id)
        {
            await _data.Users.AddAsync(new ApplicationUser
            {
                Id = id,
                UserName = $"{id}@mail.com",
                Email = $"{id}@mail.com",
                FirstName = "Test",
                LastName = "User"
            });
            await _data.SaveChangesAsync();
        }

        private async Task AddCoachWithTeamAsync(string userId, int teamId)
        {
            await AddUserAsync(userId);
            await _data.Teams.AddAsync(new Team { Id = teamId, Name = $"Team {teamId}", CoachId = userId });
            await _data.Coaches.AddAsync(new Coach { Id = userId, Name = userId, Experience = "3", TeamId = teamId });
            await _data.Players.AddRangeAsync(
                new Player { Name = "Player One", TeamId = teamId },
                new Player { Name = "Player Two", TeamId = teamId });
            await _data.SaveChangesAsync();
        }

        private async Task AddTournamentAsync(int id, DateTime start, DateTime end, string organiserId = "org-x", string? refereeId = null)
        {
            await _data.Tournaments.AddAsync(new Tournament
            {
                Id = id,
                StartDate = start,
                EndDate = end,
                Description = $"Tournament {id}",
                OrganiserId = organiserId,
                RefereeId = refereeId,
                CreatedOn = DateTime.Now,
                ImageUrl = "https://example.com/t.jpg",
                Status = TournamentStatus.Upcoming,
                ParticipationFee = 10m
            });
            await _data.TournamentsCities.AddAsync(new TournamentCity { TournamentId = id, CityId = CityId });
            await _data.SaveChangesAsync();
        }
    }
}
