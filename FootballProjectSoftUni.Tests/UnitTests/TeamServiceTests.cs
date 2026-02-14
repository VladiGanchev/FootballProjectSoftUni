using FootballProjectSoftUni.Core.Contracts.Team;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Models.Player;
using FootballProjectSoftUni.Core.Models.Team;
using FootballProjectSoftUni.Core.Services.Team;
using FootballProjectSoftUni.Core.Services.Tournament;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    public class TeamServiceTests : UnitTestsBase
    {
        private ITeamService _teamService;
        private ITournamentService _tournamentService;
        [SetUp]

        public void SetUp()
        {
            _tournamentService = new TournamentService(_data);
            _teamService = new TeamService(_data, _tournamentService);
        }

        private Tournament FinishedTournament(int id)
            => new Tournament
            {
                Id = id,
                StartDate = DateTime.Now.AddDays(-5),
                EndDate = DateTime.Now.AddDays(-1),
                CreatedOn = DateTime.Now.AddDays(-10),
                Description = "test",
                NumberOfTeams = 0,
                Status = FootballProjectSoftUni.Infrastructure.Data.Enums.TournamentStatus.Finished,
                OrganiserId = Guid.NewGuid().ToString(),
                RefereeId = null
            };

        private Coach MakeCoach(string id)
            => new Coach { Id = id, Name = "Coach", Experience = "3" };

        private Referee MakeReferee(string id)
            => new Referee { Id = id, Name = "Ref", Experience = 5, Birthdate = DateTime.Now.AddYears(-25) };

        private Tournament ActiveTournament(int id) => new Tournament
        {
            Id = id,
            StartDate = DateTime.Now.AddDays(-1),
            EndDate = DateTime.Now.AddDays(1),
            CreatedOn = DateTime.Now,
            Description = "test",
            NumberOfTeams = 0,
            Status = FootballProjectSoftUni.Infrastructure.Data.Enums.TournamentStatus.Upcoming,
            OrganiserId = Guid.NewGuid().ToString(),
            RefereeId = null
        };

        private Coach MakeCoach(string id, int? teamId = null) => new Coach
        {
            Id = id,
            Name = "Coach",
            Experience = "3",
            TeamId = teamId
        };

        private TeamRegistrationViewModel MakeTeamModel(string teamName = "NewTeam")
        {
            return new TeamRegistrationViewModel
            {
                TeamName = teamName,
                Players = Enumerable.Range(1, 10)
                    .Select(i => new PlayerViewModel { Name = $"P{i}" })
                    .ToList()
            };
        }

        private TeamRegistrationViewModel DraftModel(string name = "DraftTeam")
    => new TeamRegistrationViewModel
    {
        TeamName = name,
        Players = Enumerable.Range(1, 10)
            .Select(i => new PlayerViewModel { Name = $"P{i}" })
            .ToList()
    };


        [Test]
        public async Task CheckForErrorsAsync_TournamentNotFound_ReturnsError()
        {
            var result = await _teamService.CheckForErrorsAsync(9999, "u1");

            Assert.IsNotNull(result);
            Assert.AreEqual("Tournament not found.", result!.Message);
        }

        [Test]
        public async Task CheckForErrorsAsync_TournamentFinished_ReturnsFinishedError()
        {
            var t = FinishedTournament(1);
            _data.Tournaments.Add(t);
            await _data.SaveChangesAsync();

            var result = await _teamService.CheckForErrorsAsync(1, "u1");

            Assert.IsNotNull(result);
            Assert.AreEqual("You cannot join a tournament that has already finished.", result!.Message);
        }

        [Test]
        public async Task CheckForErrorsAsync_MaxTeamsReached_ReturnsMaxTeamsError()
        {
            var t = ActiveTournament(2);
            _data.Tournaments.Add(t);

            for (int i = 1; i <= 16; i++)
            {
                _data.TournamentsTeams.Add(new TournamentTeam
                {
                    TournamentId = t.Id,
                    TeamId = 1000 + i
                });
            }

            await _data.SaveChangesAsync();

            var result = await _teamService.CheckForErrorsAsync(t.Id, "anyUser");

            Assert.IsNotNull(result);
            Assert.AreEqual("You cannot apply for this tournament because the maximum limit of teams has been reached.",
                result!.Message);
        }

        [Test]
        public async Task CheckForErrorsAsync_UserIsRefereeInTournament_ReturnsRefereeInTournamentError()
        {
            var t = ActiveTournament(3);
            var userId = "ref1";

            _data.Tournaments.Add(t);
            _data.Referees.Add(MakeReferee(userId));

            _data.TournamentsParticipants.Add(new TournamentParticipant
            {
                ParticipantId = userId,
                TournamentId = t.Id,
                Role = "Referee"
            });

            await _data.SaveChangesAsync();

            var result = await _teamService.CheckForErrorsAsync(t.Id, userId);

            Assert.IsNotNull(result);
            Assert.AreEqual("You cannot apply as a coach for this tournament because you are already a referee in it.",
                result!.Message);
        }

        [Test]
        public async Task CheckForErrorsAsync_UserIsRefereeButNotInTournament_AndNotCoach_ReturnsNeedCoachError()
        {
            var t = ActiveTournament(4);
            var userId = "ref2";

            _data.Tournaments.Add(t);
            _data.Referees.Add(MakeReferee(userId));
            await _data.SaveChangesAsync();

            var result = await _teamService.CheckForErrorsAsync(t.Id, userId);

            Assert.IsNotNull(result);
            Assert.AreEqual("You need to become a coach to join a team.", result!.Message);
        }

        [Test]
        public async Task CheckForErrorsAsync_UserNotCoach_ReturnsNeedCoachError()
        {
            var t = ActiveTournament(5);
            _data.Tournaments.Add(t);
            await _data.SaveChangesAsync();

            var result = await _teamService.CheckForErrorsAsync(t.Id, "u-not-coach");

            Assert.IsNotNull(result);
            Assert.AreEqual("You need to become a coach to join a team.", result!.Message);
        }

        [Test]
        public async Task CheckForErrorsAsync_AlreadyCoachInSameTournament_ReturnsSecondTimeError()
        {
            var t = ActiveTournament(6);
            var userId = "coach1";

            _data.Tournaments.Add(t);
            _data.Coaches.Add(MakeCoach(userId));

            _data.TournamentsParticipants.Add(new TournamentParticipant
            {
                ParticipantId = userId,
                TournamentId = t.Id,
                Role = "Coach"
            });

            await _data.SaveChangesAsync();

            var result = await _teamService.CheckForErrorsAsync(t.Id, userId);

            Assert.IsNotNull(result);
            Assert.AreEqual("You cannot apply second time for this tournament because you already participate as a coach in it.",
                result!.Message);
        }

        [Test]
        public async Task CheckForErrorsAsync_CoachHasActiveParticipationInOtherTournament_ReturnsActiveParticipationError()
        {
            var targetTournament = ActiveTournament(7);
            var activeOtherTournament = ActiveTournament(8);
            var userId = "coach2";

            _data.Tournaments.AddRange(targetTournament, activeOtherTournament);
            _data.Coaches.Add(MakeCoach(userId));

            _data.TournamentsParticipants.Add(new TournamentParticipant
            {
                ParticipantId = userId,
                TournamentId = activeOtherTournament.Id,
                Role = "Coach"
            });

            await _data.SaveChangesAsync();

            var result = await _teamService.CheckForErrorsAsync(targetTournament.Id, userId);

            Assert.IsNotNull(result);
            Assert.AreEqual("You cannot apply for second tournament. You have to leave the current one in order to join a new one.",
                result!.Message);
        }

        [Test]
        public async Task CheckForErrorsAsync_WhenNoErrors_ReturnsNull()
        {
            var t = ActiveTournament(9);
            var userId = "coach-ok";

            _data.Tournaments.Add(t);
            _data.Coaches.Add(MakeCoach(userId));

            await _data.SaveChangesAsync();

            var result = await _teamService.CheckForErrorsAsync(t.Id, userId);

            Assert.IsNull(result);
        }

        [Test]
        public async Task CreateModel_ShouldBeTrue()
        {
            var result = _teamService.CreateModel(111);

            var model = new TeamRegistrationViewModel()
            {
                TournamentId = 111
            };

            Assert.AreEqual(result.TournamentId, model.TournamentId);
        }

        [Test]
        public async Task GetCityId_ShouldBeTrue()
        {
            var tournament = new Tournament()
            {
                Id = 35482,
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

            var result = await _teamService.GetCityIdAsync(tournament.Id);

            Assert.AreEqual(result, 1);
        }

        [Test]
        public async Task JoinTeamAsync_WhenCoachMissing_ReturnsNeedCoachError()
        {
            var t = ActiveTournament(1);
            _data.Tournaments.Add(t);
            await _data.SaveChangesAsync();

            var model = MakeTeamModel();

            var result = await _teamService.JoinTeamAsync(model, t.Id, "missing-coach");

            Assert.IsNotNull(result);
            Assert.AreEqual("You need to become a coach to join a team.", result!.Message);
        }

        [Test]
        public async Task JoinTeamAsync_WhenTournamentMissing_ReturnsBadRequest()
        {
            var coach = MakeCoach("c1");
            _data.Coaches.Add(coach);
            await _data.SaveChangesAsync();

            var model = MakeTeamModel();

            var result = await _teamService.JoinTeamAsync(model, 9999, coach.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual("BadRequest Message", result!.Message);
        }

        [Test]
        public async Task JoinTeamAsync_WhenCoachHasTeam_AddsTournamentTeamAndParticipation_ReturnsNull()
        {
            var tournament = ActiveTournament(2);

            var coach = MakeCoach("c2", teamId: 200);
            var team = new Team { Id = 200, Name = "ExistingTeam", CoachId = coach.Id, Coach = coach };

            _data.Tournaments.Add(tournament);
            _data.Coaches.Add(coach);
            _data.Teams.Add(team);

            await _data.SaveChangesAsync();

            var result = await _teamService.JoinTeamAsync(MakeTeamModel(), tournament.Id, coach.Id);

            Assert.IsNull(result);

            Assert.IsTrue(await _data.TournamentsTeams.AnyAsync(tt => tt.TournamentId == tournament.Id && tt.TeamId == team.Id));

            Assert.IsTrue(await _data.TournamentsParticipants.AnyAsync(tp =>
                tp.TournamentId == tournament.Id && tp.ParticipantId == coach.Id && tp.Role == "Coach"));

            var tFromDb = await _data.Tournaments.FirstAsync(t => t.Id == tournament.Id);
            Assert.AreEqual(1, tFromDb.NumberOfTeams);
        }

        [Test]
        public async Task JoinTeamAsync_WhenCoachHasTeam_AndAlreadyJoined_DoesNotDuplicate_ReturnsNull()
        {
            var tournament = ActiveTournament(3);

            var coach = MakeCoach("c3", teamId: 300);
            var team = new Team { Id = 300, Name = "ExistingTeam2", CoachId = coach.Id, Coach = coach };

            _data.Tournaments.Add(tournament);
            _data.Coaches.Add(coach);
            _data.Teams.Add(team);

            _data.TournamentsTeams.Add(new TournamentTeam { TournamentId = tournament.Id, TeamId = team.Id });
            _data.TournamentsParticipants.Add(new TournamentParticipant
            {
                ParticipantId = coach.Id,
                TournamentId = tournament.Id,
                Role = "Coach"
            });

            await _data.SaveChangesAsync();

            var result = await _teamService.JoinTeamAsync(MakeTeamModel(), tournament.Id, coach.Id);

            Assert.IsNull(result);

            var ttCount = await _data.TournamentsTeams.CountAsync(tt => tt.TournamentId == tournament.Id && tt.TeamId == team.Id);
            var tpCount = await _data.TournamentsParticipants.CountAsync(tp =>
                tp.TournamentId == tournament.Id && tp.ParticipantId == coach.Id && tp.Role == "Coach");

            Assert.AreEqual(1, ttCount);
            Assert.AreEqual(1, tpCount);
        }

        [Test]
        public async Task JoinTeamAsync_WhenCoachHasNoTeam_AndModelIsNull_ReturnsNoTeamYet()
        {
            var tournament = ActiveTournament(4);
            var coach = MakeCoach("c4", teamId: null);

            _data.Tournaments.Add(tournament);
            _data.Coaches.Add(coach);
            await _data.SaveChangesAsync();

            var result = await _teamService.JoinTeamAsync(null, tournament.Id, coach.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual("NO_TEAM_YET", result!.Message);
        }

        [Test]
        public async Task JoinTeamAsync_WhenCoachHasNoTeam_AndNameExists_ReturnsNameExistsError()
        {
            var tournament = ActiveTournament(5);

            var coach = new Coach
            {
                Id = "c5",
                Name = "Coach",
                Experience = "3",
                TeamId = null
            };

            _data.Tournaments.Add(tournament);
            _data.Coaches.Add(coach);
            
            _data.Teams.Add(new Team
            {
                Id = 555,
                Name = "SameName",
                CoachId = "someone-else"
            });

            await _data.SaveChangesAsync();

            var model = MakeTeamModel("SameName");

            var result = await _teamService.JoinTeamAsync(model, tournament.Id, coach.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual("A Team with the same name already exists", result!.Message);
        }

        [Test]
        public void CreateTeamDraftAsync_WhenCoachMissing_ThrowsCoachNotFound()
        {
            var model = DraftModel("Draft1");

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _teamService.CreateTeamDraftAsync(model, "missing-coach"));

            Assert.AreEqual("Coach not found.", ex!.Message);
        }

        [Test]
        public async Task CreateTeamDraftAsync_WhenNameExists_ThrowsNameExists()
        {
            var coach = new Coach { Id = "c1", Name = "Coach", Experience = "3" };
            _data.Coaches.Add(coach);

            _data.Teams.Add(new Team
            {
                Id = 100,
                Name = "SameName",
                CoachId = "someone-else"
            });

            await _data.SaveChangesAsync();

            var model = DraftModel("SameName");

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _teamService.CreateTeamDraftAsync(model, coach.Id));

            Assert.AreEqual("A Team with the same name already exists", ex!.Message);
        }

        [Test]
        public void FinalizeJoinAsync_WhenTournamentMissing_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _teamService.FinalizeJoinAsync(9999, "u1", 1, 1));
        }

        [Test]
        public async Task FinalizeJoinAsync_WhenTournamentFinished_ThrowsInvalidOperation()
        {
            var t = FinishedTournament(1);
            _data.Tournaments.Add(t);
            await _data.SaveChangesAsync();

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _teamService.FinalizeJoinAsync(t.Id, "u1", 1, 1));

            Assert.AreEqual("Tournament already finished.", ex!.Message);
        }

        [Test]
        public async Task FinalizeJoinAsync_WhenTeamMissing_ThrowsTeamNotFound()
        {
            var t = ActiveTournament(2);
            _data.Tournaments.Add(t);
            await _data.SaveChangesAsync();

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _teamService.FinalizeJoinAsync(t.Id, "u1", 999, 1));

            Assert.AreEqual("Team not found.", ex!.Message);
        }

        [Test]
        public async Task FinalizeJoinAsync_WhenValid_AddsTournamentTeamParticipationAndCityEntry()
        {
            var tournament = ActiveTournament(3);
            var coachId = "coach1";

            var coach = new Coach { Id = coachId, Name = "Coach", Experience = "3" };
            var team = new Team { Id = 30, Name = "T30", CoachId = coachId, Coach = coach };

            _data.Tournaments.Add(tournament);
            _data.Coaches.Add(coach);
            _data.Teams.Add(team);

            await _data.SaveChangesAsync();

            await _teamService.FinalizeJoinAsync(tournament.Id, coachId, team.Id, cityId: 5);

            Assert.IsTrue(await _data.TournamentsTeams.AnyAsync(tt => tt.TournamentId == tournament.Id && tt.TeamId == team.Id));

            Assert.IsTrue(await _data.TournamentsParticipants.AnyAsync(tp =>
                tp.TournamentId == tournament.Id && tp.ParticipantId == coachId && tp.Role == "Coach"));

            Assert.IsTrue(await _data.CityBestTeams.AnyAsync(cb => cb.CityId == 5 && cb.TeamId == team.Id));

            var tFromDb = await _data.Tournaments.FirstAsync(t => t.Id == tournament.Id);
            Assert.AreEqual(1, tFromDb.NumberOfTeams);
        }

        [Test]
        public async Task FinalizeJoinAsync_WhenAlreadyHasTTAndTP_DoesNotDuplicate_AddsCityEntry()
        {
            var tournament = ActiveTournament(4);
            var coachId = "coach2";

            var coach = new Coach { Id = coachId, Name = "Coach", Experience = "3" };
            var team = new Team { Id = 40, Name = "T40", CoachId = coachId, Coach = coach };

            _data.Tournaments.Add(tournament);
            _data.Coaches.Add(coach);
            _data.Teams.Add(team);

            _data.TournamentsTeams.Add(new TournamentTeam { TournamentId = tournament.Id, TeamId = team.Id });
            _data.TournamentsParticipants.Add(new TournamentParticipant
            {
                ParticipantId = coachId,
                TournamentId = tournament.Id,
                Role = "Coach"
            });

            await _data.SaveChangesAsync();

            await _teamService.FinalizeJoinAsync(tournament.Id, coachId, team.Id, cityId: 7);

            var ttCount = await _data.TournamentsTeams.CountAsync(tt => tt.TournamentId == tournament.Id && tt.TeamId == team.Id);
            var tpCount = await _data.TournamentsParticipants.CountAsync(tp =>
                tp.TournamentId == tournament.Id && tp.ParticipantId == coachId && tp.Role == "Coach");

            Assert.AreEqual(1, ttCount);
            Assert.AreEqual(1, tpCount);

            Assert.IsTrue(await _data.CityBestTeams.AnyAsync(cb => cb.CityId == 7 && cb.TeamId == team.Id));
        }

        [Test]
        public async Task FinalizeJoinAsync_WhenCityEntryExists_ReturnsEarly_DoesNotDuplicateCityEntry()
        {
            var tournament = ActiveTournament(5);
            var coachId = "coach3";

            var coach = new Coach { Id = coachId, Name = "Coach", Experience = "3" };
            var team = new Team { Id = 50, Name = "T50", CoachId = coachId, Coach = coach };

            _data.Tournaments.Add(tournament);
            _data.Coaches.Add(coach);
            _data.Teams.Add(team);

            _data.CityBestTeams.Add(new CityBestTeam
            {
                CityId = 9,
                TeamId = team.Id,
                WinsInCity = 0
            });

            await _data.SaveChangesAsync();

            await _teamService.FinalizeJoinAsync(tournament.Id, coachId, team.Id, cityId: 9);

            var count = await _data.CityBestTeams.CountAsync(cb => cb.CityId == 9 && cb.TeamId == team.Id);
            Assert.AreEqual(1, count);
        }

        [Test]
        public async Task GetCoachTeamIdAsync_WhenCoachNotFound_ReturnsNull()
        {
            var result = await _teamService.GetCoachTeamIdAsync("missing-id");

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetCoachTeamIdAsync_WhenCoachHasNoTeam_ReturnsNull()
        {
            var coach = new Coach
            {
                Id = "c2",
                Name = "Coach",
                Experience = "3",
                TeamId = null
            };

            _data.Coaches.Add(coach);
            await _data.SaveChangesAsync();

            var result = await _teamService.GetCoachTeamIdAsync(coach.Id);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetCoachTeamIdAsync_WhenCoachHasTeam_ReturnsTeamId()
        {
            var coach = new Coach
            {
                Id = "c1",
                Name = "Coach",
                Experience = "3",
                TeamId = 100
            };

            _data.Coaches.Add(coach);
            await _data.SaveChangesAsync();

            var result = await _teamService.GetCoachTeamIdAsync(coach.Id);

            Assert.AreEqual(100, result);
        }


    }
}
