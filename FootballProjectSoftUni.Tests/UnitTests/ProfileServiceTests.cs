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
        public async Task RemoveRefereeRoleAsync_ShouldRemoveReferee_AndClearTournamentRefereeId()
        {
            var referee = new Referee
            {
                Id = "ref-remove",
                Name = "Ref Remove",
                Birthdate = DateTime.Now.AddYears(-30),
                Experience = 5
            };

            var tournament = new Tournament
            {
                Id = 6001,
                StartDate = DateTime.Now.AddDays(5),
                EndDate = DateTime.Now.AddDays(10),
                Description = "Test",
                OrganiserId = "org1",
                NumberOfTeams = 0,
                CreatedOn = DateTime.Now,
                ImageUrl = "img",
                Status = TournamentStatus.Upcoming,
                Prize = 0,
                ParticipationFee = 0,
                RefereeId = "ref-remove"
            };

            await _data.Referees.AddAsync(referee);
            await _data.Tournaments.AddAsync(tournament);
            await _data.SaveChangesAsync();

            await _profileService.RemoveRefereeRoleAsync("ref-remove");

            var removedReferee = await _data.Referees.FindAsync("ref-remove");
            var updatedTournament = await _data.Tournaments.FirstAsync(t => t.Id == 6001);

            Assert.IsNull(removedReferee);
            Assert.IsNull(updatedTournament.RefereeId);
        }

        [Test]
        public async Task RemoveRefereeRoleAsync_ShouldRemoveOnlyRefereeParticipations()
        {
            var referee = new Referee
            {
                Id = "ref-part",
                Name = "Ref Part",
                Birthdate = DateTime.Now.AddYears(-35),
                Experience = 6
            };

            var tournament = new Tournament
            {
                Id = 6002,
                StartDate = DateTime.Now.AddDays(5),
                EndDate = DateTime.Now.AddDays(10),
                Description = "Test",
                OrganiserId = "org1",
                NumberOfTeams = 0,
                CreatedOn = DateTime.Now,
                ImageUrl = "img",
                Status = TournamentStatus.Upcoming,
                Prize = 0,
                ParticipationFee = 0
            };

            await _data.Referees.AddAsync(referee);
            await _data.Tournaments.AddAsync(tournament);

            await _data.TournamentsParticipants.AddRangeAsync(
                new TournamentParticipant
                {
                    ParticipantId = "ref-part",
                    TournamentId = 6002,
                    Role = "Referee"
                },
                new TournamentParticipant
                {
                    ParticipantId = "player-1",
                    TournamentId = 6002,
                    Role = "Player"
                }
            );

            await _data.SaveChangesAsync();

            await _profileService.RemoveRefereeRoleAsync("ref-part");

            var remaining = await _data.TournamentsParticipants
                .Where(tp => tp.TournamentId == 6002)
                .ToListAsync();

            Assert.That(remaining.Count, Is.EqualTo(1));
            Assert.That(remaining[0].ParticipantId, Is.EqualTo("player-1"));
            Assert.That(remaining[0].Role, Is.EqualTo("Player"));
        }


        [Test]
        public async Task RemoveRefereeRoleAsync_ShouldRemoveReferee_AndParticipations_WhenNoTournamentAssigned()
        {
            var referee = new Referee
            {
                Id = "ref-only",
                Name = "Ref Only",
                Birthdate = DateTime.Now.AddYears(-28),
                Experience = 4
            };

            await _data.Referees.AddAsync(referee);

            await _data.TournamentsParticipants.AddAsync(
                new TournamentParticipant
                {
                    ParticipantId = "ref-only",
                    TournamentId = 6003,
                    Role = "Referee"
                });

            await _data.SaveChangesAsync();

            await _profileService.RemoveRefereeRoleAsync("ref-only");

            var removedReferee = await _data.Referees.FindAsync("ref-only");
            var remainingParticipations = await _data.TournamentsParticipants
                .Where(tp => tp.ParticipantId == "ref-only")
                .ToListAsync();

            Assert.IsNull(removedReferee);
            Assert.IsEmpty(remainingParticipations);
        }



    }
}
