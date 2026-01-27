using FootballProjectSoftUni.Core.Contracts.Team;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Models.Player;
using FootballProjectSoftUni.Core.Models.Team;
using FootballProjectSoftUni.Core.Services.Team;
using FootballProjectSoftUni.Infrastructure.Data.Models;
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
        [OneTimeSetUp]

        //trqbva da se opravi posle
       // public void SetUp() => _teamService = new TeamService(_data);

        [Test]
        public async Task CheckForError_ShouldReturn_FirstError()
        {
            var tournament = new Tournament()
            {
                Id = 16,
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


            var coaches = GenerateCoaches();

            for (int i = 0; i < 16; i++)
            {
                var team = new Team()
                {
                    Id = i + 16,
                    Name = $"{i}",
                    CoachId = coaches[i].Id,
                    Coach = coaches[i]
                };

                _data.Teams.Add(team);
                _data.SaveChanges();

            }
            for (int i = 0; i < 16; i++)
            {
                var tt = new TournamentTeam()
                {
                    TeamId = i + 16,
                    TournamentId = 16,

                };

                _data.TournamentsTeams.Add(tt);
                _data.SaveChanges();

            }

            var extraCoach = new Coach()
            {
                Id = "a2bda19v-7da6-4fc4-bcc8-0c5ce80153f2",
                Name = "q",
                Experience = "3"

            };

            _data.Coaches.Add(extraCoach);
            _data.SaveChanges();

            var extraTeam = new Team()
            {
                Id = 99999,
                Name = "asan",
                CoachId = extraCoach.Id,
                Coach = extraCoach
            };

            _data.Teams.Add(extraTeam);
            _data.SaveChanges();

            var result = await _teamService.CheckForErrorsAsync(16, "wfds");

            Assert.AreEqual(result.Message, "You cannot apply for this tournament because the maximum limit of teams has been reached.");
        
        }

        private List<Coach> GenerateCoaches()
        {
            List<Coach> coaches = new List<Coach>();

            var coach1 = new Coach()
            {
                Id = "a2bda19a-7da6-4fc4-bcc8-0c5ce80153f2",
                Name = "q",
                Experience = "3"

            };

            _data.Coaches.Add(coach1);
            _data.SaveChanges();
            coaches.Add(coach1);

            var coach2 = new Coach()
            {
                Id = "b2bda19a-7da6-4fc4-bcc8-0c5ce80153f2",
                Name = "q",
                Experience = "3"

            };

            _data.Coaches.Add(coach2);
            _data.SaveChanges();
            coaches.Add(coach2);

            var coach3 = new Coach()
            {
                Id = "c2bda19a-7da6-4fc4-bcc8-0c5ce80153f2",
                Name = "q",
                Experience = "3"

            };

            _data.Coaches.Add(coach3);
            _data.SaveChanges();
            coaches.Add(coach3);


            var coach4 = new Coach()
            {
                Id = "d2bda19a-7da6-4fc4-bcc8-0c5ce80153f2",
                Name = "q",
                Experience = "3"

            };
            _data.Coaches.Add(coach4);
            _data.SaveChanges();
            coaches.Add(coach4);


            var coach5 = new Coach()
            {
                Id = "e2bda19a-7da6-4fc4-bcc8-0c5ce80153f2",
                Name = "q",
                Experience = "3"

            };
            _data.Coaches.Add(coach5);
            _data.SaveChanges();
            coaches.Add(coach5);


            var coach6 = new Coach()
            {
                Id = "f2bda19a-7da6-4fc4-bcc8-0c5ce80153f2",
                Name = "q",
                Experience = "3"

            };
            _data.Coaches.Add(coach6);
            _data.SaveChanges();
            coaches.Add(coach6);


            var coach7 = new Coach()
            {
                Id = "g2bda19a-7da6-4fc4-bcc8-0c5ce80153f2",
                Name = "q",
                Experience = "3"

            };
            _data.Coaches.Add(coach7);
            _data.SaveChanges();
            coaches.Add(coach7);


            var coach8 = new Coach()
            {
                Id = "h2bda19a-7da6-4fc4-bcc8-0c5ce80153f2",
                Name = "q",
                Experience = "3"

            };

            _data.Coaches.Add(coach8);
            _data.SaveChanges();
            coaches.Add(coach8);


            var coach9 = new Coach()
            {
                Id = "i2bda19a-7da6-4fc4-bcc8-0c5ce80153f2",
                Name = "q",
                Experience = "3"

            };

            _data.Coaches.Add(coach9);
            _data.SaveChanges();
            coaches.Add(coach9);


            var coach10 = new Coach()
            {
                Id = "j2bda19a-7da6-4fc4-bcc8-0c5ce80153f2",
                Name = "q",
                Experience = "3"

            };

            _data.Coaches.Add(coach10);
            _data.SaveChanges();
            coaches.Add(coach10);


            var coach11 = new Coach()
            {
                Id = "k2bda19a-7da6-4fc4-bcc8-0c5ce80153f2",
                Name = "q",
                Experience = "3"

            };

            _data.Coaches.Add(coach11);
            _data.SaveChanges();
            coaches.Add(coach11);


            var coach12 = new Coach()
            {
                Id = "l2bda19a-7da6-4fc4-bcc8-0c5ce80153f2",
                Name = "q",
                Experience = "3"

            };

            _data.Coaches.Add(coach12);
            _data.SaveChanges();
            coaches.Add(coach12);


            var coach13 = new Coach()
            {
                Id = "m2bda19a-7da6-4fc4-bcc8-0c5ce80153f2",
                Name = "q",
                Experience = "3"

            };

            _data.Coaches.Add(coach13);
            _data.SaveChanges();
            coaches.Add(coach13);


            var coach14 = new Coach()
            {
                Id = "n2bda19a-7da6-4fc4-bcc8-0c5ce80153f2",
                Name = "q",
                Experience = "3"

            };

            _data.Coaches.Add(coach14);
            _data.SaveChanges();
            coaches.Add(coach14);


            var coach15 = new Coach()
            {
                Id = "o2bda19a-7da6-4fc4-bcc8-0c5ce80153f2",
                Name = "q",
                Experience = "3"

            };

            _data.Coaches.Add(coach15);
            _data.SaveChanges();
            coaches.Add(coach15);


            var coach16 = new Coach()
            {
                Id = "p2bda19a-7da6-4fc4-bcc8-0c5ce80153f2",
                Name = "q",
                Experience = "3"

            };

            _data.Coaches.Add(coach16);
            _data.SaveChanges();
            coaches.Add(coach16);


            return coaches;
        }

        [Test]
        public async Task CheckForErrors_ShouldReturn_SecondError()
        {
            var tournament = new Tournament()
            {
                Id = 17,
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
                Id = "01bd2vq2-415b-4514-a6hb-11042df2acca",
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
                ParticipantId = "01bd2vq2-415b-4514-a6hb-11042df2acca",
                TournamentId = tournament.Id,
                Role = "Referee"
            };

            _data.TournamentsParticipants.Add(tp);
            _data.SaveChanges();

            var result = await _teamService.CheckForErrorsAsync(tournament.Id, "01bd2vq2-415b-4514-a6hb-11042df2acca");

            Assert.AreEqual(result.Message, "You cannot apply as a coach for this tournament because you are already a referee in it.");

        }


        [Test]
        public async Task ChechForError_ShouldReturn_ThirdError()
        {
            var tournament = new Tournament()
            {
                Id = 18,
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

            var result = await _teamService.CheckForErrorsAsync(tournament.Id, "01bd2vq2-415b-4514-a6hb-11042dd2acca");

            Assert.AreEqual(result.Message, "You need to become a coach to join a team.");
        }

        [Test]

        public async Task ChechForError_ShouldReturn_FourthError()
        {
            var tournament = new Tournament()
            {
                Id = 6545,
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


            var coach = new Coach()
            {
                Id = "a2gda19v-7dk6-43c4-bcc8-0c5ce80153f2",
                Name = "q",
                Experience = "3"

            };

            _data.Coaches.Add(coach);
            _data.SaveChanges();

            var team = new Team()
            {
                Id = 99499,
                Name = "asan",
                CoachId = coach.Id,
                Coach = coach
            };

            _data.Teams.Add(team);
            _data.SaveChanges();

            var tp = new TournamentParticipant()
            {
                ParticipantId = "a2gda19v-7dk6-43c4-bcc8-0c5ce80153f2",
                TournamentId = tournament.Id,
                Role = "Coach"
            };

            _data.TournamentsParticipants.Add(tp);
            _data.SaveChanges();

            var result = await _teamService.CheckForErrorsAsync(tournament.Id, "a2gda19v-7dk6-43c4-bcc8-0c5ce80153f2");

            Assert.AreEqual(result.Message, "You cannot apply second time for this tournament because you already participate as a coach in it.");
        }

        [Test]
        public async Task ChechForError_ShouldReturn_FifthError()
        {
            var tournament = new Tournament()
            {
                Id = 3548,
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


            var coach = new Coach()
            {
                Id = "a2gda19v-7dk6-43c4-bcc8-0c53e80153f2",
                Name = "q",
                Experience = "3"

            };

            _data.Coaches.Add(coach);
            _data.SaveChanges();

            var team = new Team()
            {
                Id = 99439,
                Name = "asan",
                CoachId = coach.Id,
                Coach = coach
            };

            _data.Teams.Add(team);
            _data.SaveChanges();

            var tp = new TournamentParticipant()
            {
                ParticipantId = "a2gda19v-7dk6-43c4-bcc8-0c53e80153f2",
                TournamentId = tournament.Id,
                Role = "Coach"
            };

            _data.TournamentsParticipants.Add(tp);
            _data.SaveChanges();

            var secondTournament = new Tournament()
            {
                Id = 3535,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                CreatedOn = DateTime.Now,
                Description = "lkjhgfdvbjkiuytrdvbnjiuytfgbnjuytghjuytg",
                NumberOfTeams = 0,
                Status = FootballProjectSoftUni.Infrastructure.Data.Enums.TournamentStatus.Finished,
                OrganiserId = "600bafb9-a73d-4489-a387-643c3b8ae96c",
                RefereeId = null
            };

            _data.Tournaments.Add(secondTournament);
            _data.SaveChanges();

            var result = await _teamService.CheckForErrorsAsync(secondTournament.Id, coach.Id);

            Assert.AreEqual(result.Message, "You cannot apply for second tournament. You have to leave the current one in order to join a new one.");
        }

        [Test]

        public async Task CheckForError_ShouldReturnNull()
        {
            var tournament = new Tournament()
            {
                Id = 3518,
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


            var coach = new Coach()
            {
                Id = "a2gda19v-7dk6-4324-bcc8-0c53e80153f2",
                Name = "q",
                Experience = "3"

            };

            _data.Coaches.Add(coach);
            _data.SaveChanges();

            var result = await _teamService.CheckForErrorsAsync(tournament.Id, coach.Id);

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

        public async Task JoinTeam_ShouldReturnError_ForFormat()
        {
            var tournament = new Tournament()
            {
                Id = 354812,
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

            var coach = new Coach()
            {
                Id = "ebfdvscxfvgbhtnjmkmujnhbgf",
                Name = "mitaka",
            };

            _data.Coaches.Add(coach);
            _data.SaveChanges();

            var teamModel = new TeamRegistrationViewModel()
            {
                TeamName = "omryzna mi",
                TournamentId = tournament.Id,
                Players = CreatePlayerModelsWithWrongDateFormat()
            };

            var result = await _teamService.JoinTeamAsync(teamModel, tournament.Id, coach.Id);

            Assert.AreEqual(result.Message, $"Invalid date, format must be dd/MM/yyyy HH:mm");
        }

        [Test]

        public async Task JoinTeam_ShouldReturnError_ForYears()
        {
            var tournament = new Tournament()
            {
                Id = 354862,
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

            var coach = new Coach()
            {
                Id = "ebfdvscxfvgbh2tnjmkmujnhbgf",
                Name = "mitaka",
            };

            _data.Coaches.Add(coach);
            _data.SaveChanges();

            var teamModel = new TeamRegistrationViewModel()
            {
                TeamName = "omryzna mi",
                TournamentId = tournament.Id,
                Players = CreatePlayerModelsWithRightDateFormatButWrongAge()
            };

            var result = await _teamService.JoinTeamAsync(teamModel, tournament.Id, coach.Id);

            Assert.AreEqual(result.Message, "Players must be at least 18 years old to participate.");
        }

        private List<PlayerViewModel> CreatePlayerModelsWithWrongDateFormat()
        {
            List<PlayerViewModel> models = new List<PlayerViewModel>();

            var model1 = new PlayerViewModel()
            {
                Name = "1",
            };
            var model2 = new PlayerViewModel()
            {
                Name = "2",
            };
            var model3 = new PlayerViewModel()
            {
                Name = "3",
            };
            var model4 = new PlayerViewModel()
            {
                Name = "4",
            };
            var model5 = new PlayerViewModel()
            {
                Name = "5",
            };
            var model6 = new PlayerViewModel()
            {
                Name = "6",
            };
            var model7 = new PlayerViewModel()
            {
                Name = "7",
            };
            var model8 = new PlayerViewModel()
            {
                Name = "8",
            };
            var model9 = new PlayerViewModel()
            {
                Name = "9",
            };
            var model10 = new PlayerViewModel()
            {
                Name = "10",
            };

            models.Add(model1);
            models.Add(model2);
            models.Add(model3);
            models.Add(model4);
            models.Add(model5);
            models.Add(model6);
            models.Add(model7);
            models.Add(model8);
            models.Add(model9);
            models.Add(model10);



            return models;
        }

        private List<PlayerViewModel> CreatePlayerModelsWithRightDateFormatButWrongAge()
        {
            List<PlayerViewModel> models = new List<PlayerViewModel>();

            var model1 = new PlayerViewModel()
            {
                Name = "1",
            };
            var model2 = new PlayerViewModel()
            {
                Name = "2",
            };
            var model3 = new PlayerViewModel()
            {
                Name = "3",
            };
            var model4 = new PlayerViewModel()
            {
                Name = "4",
            };
            var model5 = new PlayerViewModel()
            {
                Name = "5",
            };
            var model6 = new PlayerViewModel()
            {
                Name = "6",
            };
            var model7 = new PlayerViewModel()
            {
                Name = "7",
            };
            var model8 = new PlayerViewModel()
            {
                Name = "8",
            };
            var model9 = new PlayerViewModel()
            {
                Name = "9",
            };
            var model10 = new PlayerViewModel()
            {
                Name = "10",
            };

            models.Add(model1);
            models.Add(model2);
            models.Add(model3);
            models.Add(model4);
            models.Add(model5);
            models.Add(model6);
            models.Add(model7);
            models.Add(model8);
            models.Add(model9);
            models.Add(model10);



            return models;
        }

        private List<PlayerViewModel> CreatePlayerModelsWithRightDateFormat()
        {
            List<PlayerViewModel> models = new List<PlayerViewModel>();

            var model1 = new PlayerViewModel()
            {
                Name = "1",
                
            };
            var model2 = new PlayerViewModel()
            {
                Name = "2",
            };
            var model3 = new PlayerViewModel()
            {
                Name = "3",
            };
            var model4 = new PlayerViewModel()
            {
                Name = "4",
            };
            var model5 = new PlayerViewModel()
            {
                Name = "5",
            };
            var model6 = new PlayerViewModel()
            {
                Name = "6",
            };
            var model7 = new PlayerViewModel()
            {
                Name = "7",
            };
            var model8 = new PlayerViewModel()
            {
                Name = "8",
            };
            var model9 = new PlayerViewModel()
            {
                Name = "9",
            };
            var model10 = new PlayerViewModel()
            {
                Name = "10",
            };

            models.Add(model1);
            models.Add(model2);
            models.Add(model3);
            models.Add(model4);
            models.Add(model5);
            models.Add(model6);
            models.Add(model7);
            models.Add(model8);
            models.Add(model9);
            models.Add(model10);



            return models;
        }

    }
}
