using FootballProjectSoftUni.Core.Contracts.Coach;
using FootballProjectSoftUni.Core.Models.Coach;
using FootballProjectSoftUni.Core.Models.Tournament;
using FootballProjectSoftUni.Core.Services.City;
using FootballProjectSoftUni.Core.Services.Coach;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    public class CoachServiceTests : UnitTestsBase
    {
        private ICoachService _coachService;

        [OneTimeSetUp]
        public void SetUp() => _coachService = new CoachService(_data);

        [Test]
        public async Task BecomeCoach_ShouldCreateCoach()
        {
            string id = "2471e1da-a60e-466e-bf7f-a59eccf4b60b";
            CoachViewModel model = new CoachViewModel()
            {
                Id = id,
                Name = "Pesho",
                Experience = "3"
            };

            await _coachService.BecomeCoachAsync(model, id);

            var coach =  await _data.Coaches.FirstOrDefaultAsync(x => x.Id == id);

            Assert.AreEqual(coach.Name, model.Name);


        }

        [Test]
        public async Task CheckForError_ShouldReturnError()
        {
            string id = "2471e1da-a60e-466e-bf7f-a59eccf4b60c";
            CoachViewModel model = new CoachViewModel()
            {
                Id = id,
                Name = "Pesho",
                Experience = "3"
            };

            await _coachService.BecomeCoachAsync(model, id);

            var error = await _coachService.CheckForErrorsAsync(id);

            Assert.AreEqual(error.Message, "You have already registered as a coach.");
        }

        [Test]
        public async Task CheckForError_ShouldReturnNull()
        {
            string existingId = "2471e1da-a60e-476d-bf7f-a59eccf4b60b";
            CoachViewModel existingModel = new CoachViewModel()
            {
                Id = existingId,
                Name = "Pesho",
                Experience = "3"
            };

            await _coachService.BecomeCoachAsync(existingModel, existingId);

            string newId = "2471e1da-a60e-466e-bf7f-a59eccf5b60c"; 
            CoachViewModel newModel = new CoachViewModel()
            {
                Id = newId,
                Name = "NewPesho",
                Experience = "3"
            };

            var error = await _coachService.CheckForErrorsAsync(newId);

            Assert.IsNull(error);
        }

        [Test]

        public async Task GetAllTournamentsToParticipateAsCoachAsync_ShouldBeCorrect()
        {
            var tournament = new Tournament()
            {
                Id = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                CreatedOn = DateTime.Now,
                Description = "lkjhgfdvbjkiuytrdvbnjiuytfgbnjuytghjuytg",
                NumberOfTeams = 0,
                Status = FootballProjectSoftUni.Infrastructure.Data.Enums.TournamentStatus.Finished,
                OrganiserId = "600bafb9-a73d-4489-a387-643c2b8ae96c",
                RefereeId = null
            };

            _data.Tournaments.Add(tournament);

            _data.SaveChanges();

            var cityTournament = new TournamentCity()
            {
                CityId = 2,
                TournamentId = tournament.Id
            };

            _data.TournamentsCities.Add(cityTournament);

            _data.SaveChanges();


            string id = "2424e1da-a60e-466e-bf7f-a59eccf4b60c";
            CoachViewModel model = new CoachViewModel()
            {
                Id = id,
                Name = "Pesho",
                Experience = "3"
            };

            await _coachService.BecomeCoachAsync(model, id);

            var coach = await _data.Coaches.FirstOrDefaultAsync(x => x.Id == id);

            var players = new List<Player>()
            {
                new Player()
                {
                    Id = 1,
                    Name = "1",
                    BirthDate = DateTime.Now,
                    TeamId = 0
                },
                new Player()
                {
                    Id = 2,
                    Name = "2",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 3,
                    Name = "3",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 4,
                    Name = "4",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 5,
                    Name = "5",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 6,
                    Name = "6",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 7,
                    Name = "7",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 8,
                    Name = "8",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 9,
                    Name = "9",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 10,
                    Name = "10",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
            };

            _data.Players.AddRange(players);

            _data.SaveChanges();

            var team = new Team()
            {
                Id = 1,
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


            var tournamentsToParticipate = await _coachService.GetAllTournamentsToParticipateAsCoachAsync(coach.Id);


            Assert.AreEqual(tournamentsToParticipate.Count(), 1);


        }

        [Test]
        public async Task LeaveTournament_ShouldReturnNull()
        {
            var tournament = new Tournament()
            {
                Id = 2,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                CreatedOn = DateTime.Now,
                Description = "lkjhgfdvbjkiuytrdvbnjiuytfgbnjuytghjuytg",
                NumberOfTeams = 0,
                Status = FootballProjectSoftUni.Infrastructure.Data.Enums.TournamentStatus.Finished,
                OrganiserId = "600bafb9-a73d-4489-a387-643c2b8ae96c",
                RefereeId = null
            };

            _data.Tournaments.Add(tournament);

            _data.SaveChanges();

            var cityTournament = new TournamentCity()
            {
                CityId = 2,
                TournamentId = tournament.Id
            };

            _data.TournamentsCities.Add(cityTournament);

            _data.SaveChanges();


            string id = "2424e1da-a60e-466e-bf7f-a59eccf4b60b";
            CoachViewModel model = new CoachViewModel()
            {
                Id = id,
                Name = "Pesho",
                Experience = "3"
            };

            await _coachService.BecomeCoachAsync(model, id);

            var coach = await _data.Coaches.FirstOrDefaultAsync(x => x.Id == id);

            var players = new List<Player>()
            {
                new Player()
                {
                    Id = 11,
                    Name = "1",
                    BirthDate = DateTime.Now,
                    TeamId = 0
                },
                new Player()
                {
                    Id = 12,
                    Name = "2",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 13,
                    Name = "3",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 14,
                    Name = "4",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 15,
                    Name = "5",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 16,
                    Name = "6",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 17,
                    Name = "7",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 18,
                    Name = "8",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 19,
                    Name = "9",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 20,
                    Name = "10",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
            };

            _data.Players.AddRange(players);

            _data.SaveChanges();

            var team = new Team()
            {
                Id = 2,
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

            var result1 = await _coachService.LeaveTournamentAsync(tournament.Id, "2424e1da-a60e-466e-bf7f-a59eccf4b60f");

            Assert.AreEqual(result1, false);

            var result2 = await _coachService.LeaveTournamentAsync(121, "2424e1da-a60e-466e-bf7f-a59eccf4b60b");

            Assert.AreEqual(result2, false);
        }

        [Test]
        public async Task LeaveTournament_ShouldReturnNullForCoach()
        {
            var tournament = new Tournament()
            {
                Id = 3,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                CreatedOn = DateTime.Now,
                Description = "lkjhgfdvbjkiuytrdvbnjiuytfgbnjuytghjuytg",
                NumberOfTeams = 0,
                Status = FootballProjectSoftUni.Infrastructure.Data.Enums.TournamentStatus.Finished,
                OrganiserId = "600bafb9-a73d-4489-a387-643c2b8ae96c",
                RefereeId = null
            };

            _data.Tournaments.Add(tournament);

            _data.SaveChanges();

            var cityTournament = new TournamentCity()
            {
                CityId = 2,
                TournamentId = tournament.Id
            };

            _data.TournamentsCities.Add(cityTournament);

            _data.SaveChanges();


            string id = "2424e1da-a60e-466e-bf7f-a59eccf4b60s";
            CoachViewModel model = new CoachViewModel()
            {
                Id = id,
                Name = "Pesho",
                Experience = "3"
            };

            await _coachService.BecomeCoachAsync(model, id);

            var coach = await _data.Coaches.FirstOrDefaultAsync(x => x.Id == id);

            var players = new List<Player>()
            {
                new Player()
                {
                    Id = 21,
                    Name = "1",
                    BirthDate = DateTime.Now,
                    TeamId = 0
                },
                new Player()
                {
                    Id = 22,
                    Name = "2",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 23,
                    Name = "3",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 24,
                    Name = "4",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 25,
                    Name = "5",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 26,
                    Name = "6",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 27,
                    Name = "7",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 28,
                    Name = "8",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 29,
                    Name = "9",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 30,
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
                ParticipantId = "rthbvcrew",
                TournamentId = tournament.Id,
                Role = "Coach"

            };

            _data.TournamentsParticipants.Add(tp);

            _data.SaveChanges();

            var result1 = await _coachService.LeaveTournamentAsync(tournament.Id, tp.ParticipantId);

            Assert.AreEqual(result1, false);

        }

        [Test]
        public async Task LeaveTournament_ShouldReturnNullForTeam()
        {
            var tournament = new Tournament()
            {
                Id = 5,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                CreatedOn = DateTime.Now,
                Description = "lkjhgfdvbjkiuytrdvbnjiuytfgbnjuytghjuytg",
                NumberOfTeams = 0,
                Status = FootballProjectSoftUni.Infrastructure.Data.Enums.TournamentStatus.Finished,
                OrganiserId = "600bafb9-a73d-4489-a387-643c2b8ae96c",
                RefereeId = null
            };

            _data.Tournaments.Add(tournament);

            _data.SaveChanges();

            var cityTournament = new TournamentCity()
            {
                CityId = 2,
                TournamentId = tournament.Id
            };

            _data.TournamentsCities.Add(cityTournament);

            _data.SaveChanges();


            string id = "2424e2da-a60g-466e-bf7f-a59eccf4b60s";
            CoachViewModel model = new CoachViewModel()
            {
                Id = id,
                Name = "Pesho",
                Experience = "3"
            };

            await _coachService.BecomeCoachAsync(model, id);

            var coach = await _data.Coaches.FirstOrDefaultAsync(x => x.Id == id);

            var players = new List<Player>()
            {
                new Player()
                {
                    Id = 41,
                    Name = "1",
                    BirthDate = DateTime.Now,
                    TeamId = 0
                },
                new Player()
                {
                    Id = 42,
                    Name = "2",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 43,
                    Name = "3",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 44,
                    Name = "4",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 45,
                    Name = "5",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 46,
                    Name = "6",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 47,
                    Name = "7",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 48,
                    Name = "8",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 49,
                    Name = "9",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
                new Player()
                {
                    Id = 50,
                    Name = "10",
                    BirthDate = DateTime.Now,
                    TeamId = 0

                },
            };

            _data.Players.AddRange(players);

            _data.SaveChanges();

            var team = new Team()
            {
                Id = 5,
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

            var result1 = await _coachService.LeaveTournamentAsync(tournament.Id, "2424e2da-a60g-466e-bf7f-a59eccf4b60s");

            Assert.AreEqual(result1, true);

            _data.Teams.Remove(team);

            var result2 = await _coachService.LeaveTournamentAsync(tournament.Id, "2424e2da-a60g-466e-bf7f-a59eccf4b60s");

            Assert.AreEqual(result2, false);




        }


        


    }


}
