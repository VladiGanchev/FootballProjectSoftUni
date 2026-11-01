using FootballProjectSoftUni.Core.Contracts.Profile;
using FootballProjectSoftUni.Core.Services.Profile;
using FootballProjectSoftUni.Core.Services.Tournament;
using FootballProjectSoftUni.Infrastructure.Data.Enums;
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
    public class ProfileServiceTests : UnitTestsBase
    {
        private IProfileService _profileService;
        [SetUp]
        public void SetUp() => _profileService = new ProfileService(_data);

        [Test]
        public async Task GetProfileAsync_ShouldReturnNull_WhenUserNotFound()
        {
            var result = await _profileService.GetProfileAsync("missing-user");

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetProfileAsync_ShouldReturnBasicUserInfo_WhenUserExistsWithoutRoles()
        {
            var user = new ApplicationUser
            {
                Id = "user1",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@doe.com"
            };

            await _data.Users.AddAsync(user);
            await _data.SaveChangesAsync();

            var result = await _profileService.GetProfileAsync("user1");

            Assert.NotNull(result);
            Assert.That(result!.FirstName, Is.EqualTo("John"));
            Assert.That(result.LastName, Is.EqualTo("Doe"));
            Assert.That(result.Email, Is.EqualTo("john@doe.com"));
            Assert.IsNull(result.Role);
        }

        [Test]
        public async Task GetProfileAsync_ShouldReturnCoachProfile_WithTeamName()
        {
            var user = new ApplicationUser
            {
                Id = "coach1",
                FirstName = "Coach",
                LastName = "Smith",
                Email = "coach@test.com"
            };

            var team = new Team { Id = 10, Name = "TeamX" };

            var coach = new Coach
            {
                Id = "coach1",
                Experience = "5",
                TeamId = 10
            };

            await _data.Users.AddAsync(user);
            await _data.Teams.AddAsync(team);
            await _data.Coaches.AddAsync(coach);
            await _data.SaveChangesAsync();

            var result = await _profileService.GetProfileAsync("coach1");

            Assert.NotNull(result);
            Assert.That(result!.Role, Is.EqualTo("Coach"));
            Assert.That(result.Experience, Is.EqualTo(5));
            Assert.That(result.TeamName, Is.EqualTo("TeamX"));
        }

        [Test]
        public async Task GetProfileAsync_ShouldSetCoachExperienceToZero_WhenInvalidNumber()
        {
            var user = new ApplicationUser
            {
                Id = "coach2",
                FirstName = "Coach",
                LastName = "Invalid",
                Email = "coach2@test.com"
            };

            var coach = new Coach
            {
                Id = "coach2",
                Experience = "invalid",
                TeamId = null
            };

            await _data.Users.AddAsync(user);
            await _data.Coaches.AddAsync(coach);
            await _data.SaveChangesAsync();

            var result = await _profileService.GetProfileAsync("coach2");

            Assert.NotNull(result);
            Assert.That(result!.Role, Is.EqualTo("Coach"));
            Assert.That(result.Experience, Is.EqualTo(0));
        }

        [Test]
        public async Task GetProfileAsync_ShouldReturnRefereeProfile_WithRatingsAverage()
        {
            var user = new ApplicationUser
            {
                Id = "ref1",
                FirstName = "Ref",
                LastName = "Lee",
                Email = "ref@test.com"
            };

            var referee = new Referee
            {
                Id = "ref1",
                Name = "Ref Lee",
                Birthdate = DateTime.Now.AddYears(-30),
                Experience = 7,
                RefereedTournamentsCount = 3
            };

            await _data.Users.AddAsync(user);
            await _data.Referees.AddAsync(referee);
            await AddFinishedTournamentsAsync("ref1", 3, firstId: 6101);

            await _data.RefereesRatings.AddRangeAsync(
                new RefereeRating { RefereeId = "ref1", UserId = "rater1", Value = 4 },
                new RefereeRating { RefereeId = "ref1", UserId = "rater2", Value = 5 }
            );

            await _data.SaveChangesAsync();

            var result = await _profileService.GetProfileAsync("ref1");

            Assert.NotNull(result);
            Assert.That(result!.Role, Is.EqualTo("Referee"));
            Assert.That(result.Experience, Is.EqualTo(7));
            Assert.That(result.RefereeTournamentsCount, Is.EqualTo(3));
            Assert.That(result.RefereeRating, Is.EqualTo(4.5));
        }


        [Test]
        public async Task GetProfileAsync_ShouldPrioritizeRefereeRole_WhenUserIsBothCoachAndReferee()
        {
            var user = new ApplicationUser
            {
                Id = "dual1",
                FirstName = "Dual",
                LastName = "Role",
                Email = "dual@test.com",
                UserName = "dual1",
                NormalizedUserName = "DUAL1",
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var team = new Team { Id = 5010, Name = "Dual Team" };

            var coach = new Coach
            {
                Id = "dual1",
                Experience = "3",
                TeamId = 5010,
                Team = team
            };

            var referee = new Referee
            {
                Id = "dual1",
                Name = "Dual Ref",
                Birthdate = DateTime.Now.AddYears(-25),
                Experience = 8,
                RefereedTournamentsCount = 2,
                Ratings = new System.Collections.Generic.List<RefereeRating>()
            };

            await _data.Teams.AddAsync(team);
            await _data.Users.AddAsync(user);
            await _data.Coaches.AddAsync(coach);
            await _data.Referees.AddAsync(referee);
            await AddFinishedTournamentsAsync("dual1", 2, firstId: 6201);
            await _data.SaveChangesAsync();

            var result = await _profileService.GetProfileAsync("dual1");

            Assert.NotNull(result);
            Assert.That(result!.Role, Is.EqualTo("Referee"));
            Assert.That(result.Experience, Is.EqualTo(8));
            Assert.That(result.RefereeTournamentsCount, Is.EqualTo(2));
            Assert.That(result.TeamName, Is.EqualTo("Dual Team"));
        }

        [Test]
        public async Task RemoveRefereeRoleAsync_ShouldReturn_WhenRefereeDoesNotExist()
        {
            Assert.DoesNotThrowAsync(async () =>
                await _profileService.RemoveRefereeRoleAsync("missing-ref"));
        }

        [Test]
        public async Task RemoveRefereeRoleAsync_WhileAssignedToAnUpcomingTournament_IsRefused()
        {
            await _data.Referees.AddAsync(new Referee { Id = "ref-remove", Name = "Ref Remove", Birthdate = DateTime.Now.AddYears(-30), Experience = 5 });
            await _data.Tournaments.AddAsync(ProfileTournament(6001, DateTime.Now.AddDays(5), refereeId: "ref-remove"));
            await _data.SaveChangesAsync();

            var error = await _profileService.RemoveRefereeRoleAsync("ref-remove");

            Assert.IsNotNull(error);
            StringAssert.Contains("upcoming tournament", error!.Message);
            Assert.IsTrue((await _data.Referees.FirstAsync(r => r.Id == "ref-remove")).IsActive);
            Assert.That((await _data.Tournaments.FirstAsync(t => t.Id == 6001)).RefereeId, Is.EqualTo("ref-remove"));
        }

        [Test]
        public async Task RemoveRefereeRoleAsync_WhileTheTournamentIsRunning_IsRefused()
        {
            await _data.Referees.AddAsync(new Referee { Id = "ref-part", Name = "Ref Part", Birthdate = DateTime.Now.AddYears(-35), Experience = 6 });
            await _data.Tournaments.AddAsync(ProfileTournament(6002, DateTime.Now.AddHours(-1)));
            await _data.TournamentsParticipants.AddAsync(new TournamentParticipant { ParticipantId = "ref-part", TournamentId = 6002, Role = "Referee" });
            await _data.SaveChangesAsync();

            var error = await _profileService.RemoveRefereeRoleAsync("ref-part");

            Assert.IsNotNull(error);
            StringAssert.Contains("in progress", error!.Message);
        }

        [Test]
        public async Task RemoveRefereeRoleAsync_AfterFinishedTournaments_SwitchesTheProfileOff_AndKeepsEverything()
        {
            await _data.Referees.AddAsync(new Referee { Id = "ref-only", Name = "Ref Only", Birthdate = DateTime.Now.AddYears(-28), Experience = 4, TournamentId = 6003 });
            await _data.Tournaments.AddAsync(ProfileTournament(6003, DateTime.Now.AddDays(-5), refereeId: "ref-only"));
            await _data.TournamentsParticipants.AddAsync(new TournamentParticipant { ParticipantId = "ref-only", TournamentId = 6003, Role = "Referee" });
            await _data.RefereesRatings.AddAsync(new RefereeRating { RefereeId = "ref-only", UserId = "rater", Value = 2 });
            await _data.SaveChangesAsync();

            var error = await _profileService.RemoveRefereeRoleAsync("ref-only");

            Assert.IsNull(error);
            var referee = await _data.Referees.FirstAsync(r => r.Id == "ref-only");
            Assert.IsFalse(referee.IsActive);
            Assert.IsNull(referee.TournamentId);
            Assert.That(await _data.RefereesRatings.CountAsync(r => r.RefereeId == "ref-only"), Is.EqualTo(1), "Ratings stay.");
            Assert.IsTrue(await _data.TournamentsParticipants.AnyAsync(tp => tp.ParticipantId == "ref-only"), "History stays.");
            Assert.That((await _data.Tournaments.FirstAsync(t => t.Id == 6003)).RefereeId, Is.EqualTo("ref-only"));
        }

        private static Tournament ProfileTournament(int id, DateTime start, string? refereeId = null) => new Tournament
        {
            Id = id,
            StartDate = start,
            EndDate = start.AddHours(8),
            Description = "Test",
            OrganiserId = "org1",
            NumberOfTeams = 0,
            CreatedOn = DateTime.Now,
            ImageUrl = "img",
            Status = TournamentStatus.Upcoming,
            Prize = 0,
            ParticipationFee = 0,
            RefereeId = refereeId
        };

        private async Task AddFinishedTournamentsAsync(string refereeId, int count, int firstId)
        {
            for (var i = 0; i < count; i++)
            {
                await _data.Tournaments.AddAsync(ProfileTournament(firstId + i, DateTime.Now.AddDays(-10 - i), refereeId));
            }
        }

        [Test]
        public async Task NewTournamentEmails_AreOffUntilTurnedOn_AndShowInTheProfile()
        {
            await _data.Users.AddAsync(new ApplicationUser { Id = "mail-user", Email = "m@test.com", UserName = "m@test.com", FirstName = "M", LastName = "U", SecurityStamp = "stamp" });
            await _data.SaveChangesAsync();

            Assert.That((await _profileService.GetProfileAsync("mail-user"))!.NewTournamentEmails, Is.False);

            await _profileService.SetNewTournamentEmailsAsync("mail-user", true);
            Assert.That((await _profileService.GetProfileAsync("mail-user"))!.NewTournamentEmails, Is.True);

            await _profileService.SetNewTournamentEmailsAsync("mail-user", false);
            Assert.That((await _profileService.GetProfileAsync("mail-user"))!.NewTournamentEmails, Is.False);
        }

        [Test]
        public async Task Unsubscribe_WorksOnlyWithTheKeyOfThatAccount()
        {
            await _data.Users.AddRangeAsync(
                new ApplicationUser { Id = "sub-a", Email = "a@test.com", UserName = "a@test.com", FirstName = "A", LastName = "A", SecurityStamp = "stamp-a", NewTournamentEmails = true },
                new ApplicationUser { Id = "sub-b", Email = "b@test.com", UserName = "b@test.com", FirstName = "B", LastName = "B", SecurityStamp = "stamp-b", NewTournamentEmails = true });
            await _data.SaveChangesAsync();

            var keyA = FootballProjectSoftUni.Core.Extensions.UnsubscribeKey.For("sub-a", "stamp-a");

            // Somebody else's key, a made-up key, no key, an unknown account: nothing changes.
            Assert.That(await _profileService.UnsubscribeAsync("sub-b", keyA), Is.False);
            Assert.That(await _profileService.UnsubscribeAsync("sub-a", "made-up"), Is.False);
            Assert.That(await _profileService.UnsubscribeAsync("sub-a", null), Is.False);
            Assert.That(await _profileService.UnsubscribeAsync("nobody", keyA), Is.False);
            Assert.That(await _data.Users.CountAsync(u => u.NewTournamentEmails), Is.EqualTo(2));

            Assert.That(await _profileService.IsUnsubscribeKeyValidAsync("sub-a", keyA), Is.True);
            Assert.That(await _profileService.UnsubscribeAsync("sub-a", keyA), Is.True);

            Assert.That((await _data.Users.AsNoTracking().FirstAsync(u => u.Id == "sub-a")).NewTournamentEmails, Is.False);
            Assert.That((await _data.Users.AsNoTracking().FirstAsync(u => u.Id == "sub-b")).NewTournamentEmails, Is.True);
        }

        [Test]
        public void UnsubscribeKey_NeedsASecurityStamp_AndChangesWithIt()
        {
            Assert.That(FootballProjectSoftUni.Core.Extensions.UnsubscribeKey.For("u", null), Is.Null);
            Assert.That(FootballProjectSoftUni.Core.Extensions.UnsubscribeKey.For("u", ""), Is.Null);
            Assert.That(FootballProjectSoftUni.Core.Extensions.UnsubscribeKey.IsValid("u", null, "x"), Is.False);

            // A new password gives the account a new security stamp: old links stop working.
            var before = FootballProjectSoftUni.Core.Extensions.UnsubscribeKey.For("u", "old-stamp");
            Assert.That(FootballProjectSoftUni.Core.Extensions.UnsubscribeKey.IsValid("u", "new-stamp", before), Is.False);
            Assert.That(FootballProjectSoftUni.Core.Extensions.UnsubscribeKey.IsValid("u", "old-stamp", before), Is.True);
        }
    }
}
