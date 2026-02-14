using FootballProjectSoftUni.Core.Contracts.City;
using FootballProjectSoftUni.Core.Contracts.Coach;
using FootballProjectSoftUni.Core.Contracts.Referee;
using FootballProjectSoftUni.Core.Models.Referee;
using FootballProjectSoftUni.Core.Models.Tournament;
using FootballProjectSoftUni.Core.Services.City;
using FootballProjectSoftUni.Core.Services.Referee;
using FootballProjectSoftUni.Infrastructure.Data.Enums;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    public  class RefereeServiceTests : UnitTestsBase
    {
        private IRefereeService _refereeService;

        [SetUp]

        public void SetUp() => _refereeService = new RefereeService(_data);

        [Test]

        public async Task CheckForErrors_ShouldThrowFirstError()
        {
            var tournament = new Tournament()
            {
                Id = 7,
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

            var cityTournament = new TournamentCity()
            {
                CityId = 6,
                TournamentId = tournament.Id
            };

            _data.TournamentsCities.Add(cityTournament);

            _data.SaveChanges();


            string id = "2424e1da-a66e-466e-bf7f-a59ecef4b60b";
            var coach = new Coach()
            {
                Id = id,
                Name = "Pesho",
                Experience = "3"
            };

            _data.Coaches.Add(coach);

            _data.SaveChanges();

            var tp = new TournamentParticipant()
            {
                ParticipantId = id,
                TournamentId = tournament.Id,
                Role = "Coach"

            };

            _data.TournamentsParticipants.Add(tp);

            _data.SaveChanges();

            var result = await _refereeService.CheckForErrorsAsync(tournament.Id, id);

            Assert.AreEqual(result.Message, "You cannot apply as a referee for this tournament because you already participate as a coach in it.");
        }

        [Test]
        public async Task CheckForErrorsAsync_WhenUserIsCoachInSystemButNotInTournament_ReturnsSecondError()
        {
            int tournamentId = 1;
            string userId = "u2";

            _data.Coaches.Add(new Coach
            {
                Id = userId,
                TeamId = null
            });

            await _data.SaveChangesAsync();

            var err = await _refereeService.CheckForErrorsAsync(tournamentId, userId);

            Assert.IsNotNull(err);
            Assert.AreEqual("You cannot become a referee because you are already registered as a coach in the system.",err.Message);
        }

        [Test]
        public async Task CheckForErrors_ShouldThrowSecondError()
        {
            var firstTournament = new Tournament()
            {
                Id = 9,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                CreatedOn = DateTime.Now,
                Description = "lkjhgfdvbjkiuytrdvbnjiuytfgbnjuytghjuytg",
                NumberOfTeams = 0,
                Status = FootballProjectSoftUni.Infrastructure.Data.Enums.TournamentStatus.Finished,
                OrganiserId = "600bafb9-a73d-4489-a387-643c3b8ae96c",
                RefereeId = null
            };

            _data.Tournaments.Add(firstTournament);

            _data.SaveChanges();

            var secondTournament = new Tournament()
            {
                Id = 10,
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

            var cityTournament = new TournamentCity()
            {
                CityId = 7,
                TournamentId = firstTournament.Id
            };

            _data.TournamentsCities.Add(cityTournament);

            _data.SaveChanges();

            var cityTournamentt = new TournamentCity()
            {
                CityId = 7,
                TournamentId = secondTournament.Id
            };

            _data.TournamentsCities.Add(cityTournamentt);

            _data.SaveChanges();

            var referee = new Referee()
            {
                Id = "01bd2fe2-415b-4514-a61b-11042df2accc",
                Birthdate = DateTime.Now.AddYears(-20),
                Name = "previousreferee",
                Experience = 5,
                Tournament = firstTournament,
                TournamentId = firstTournament.Id
            };

            _data.Referees.Add(referee);

            _data.SaveChanges();

            firstTournament.Referee = referee;
            _data.SaveChanges();


            var result = await _refereeService.CheckForErrorsAsync(secondTournament.Id, referee.Id);

            Assert.AreEqual(result.Message, "You cannot apply as a referee for this tournament because you already participate in another one as a referee. You have to leave the current tournament you are in to become a referee to another one.");
        }

        [Test]
        public async Task CheckForErrors_ShouldThrowThirdError()
        {
            var firstTournament = new Tournament()
            {
                Id = 11,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                CreatedOn = DateTime.Now,
                Description = "lkjhgfdvbjkiuytrdvbnjiuytfgbnjuytghjuytg",
                NumberOfTeams = 0,
                Status = FootballProjectSoftUni.Infrastructure.Data.Enums.TournamentStatus.Finished,
                OrganiserId = "600bafb9-a73d-4489-a387-643c3b8ae96c",
                RefereeId = null
            };

            _data.Tournaments.Add(firstTournament);

            _data.SaveChanges();

            var cityTournament = new TournamentCity()
            {
                CityId = 8,
                TournamentId = firstTournament.Id
            };

            _data.TournamentsCities.Add(cityTournament);

            _data.SaveChanges();

            var referee = new Referee()
            {
                Id = "01bd2fe2-415b-4514-a61b-11042df2acca",
                Birthdate = DateTime.Now.AddYears(-20),
                Name = "goshow",
                Experience = 5,
                Tournament = firstTournament,
                TournamentId = firstTournament.Id
            };

            _data.Referees.Add(referee);

            _data.SaveChanges();

            firstTournament.Referee = referee;
            firstTournament.RefereeId = referee.Id;
            _data.SaveChanges();

            var secondReferee = new Referee()
            {
                Id = "01bd2fe2-415b-3514-a61b-11052df2acbc",
                Birthdate = DateTime.Now.AddYears(-20),
                Name = "wrongreferee",
                Experience = 5,
            };

            _data.Referees.Add(secondReferee);

            _data.SaveChanges();



            var result = await _refereeService.CheckForErrorsAsync(firstTournament.Id, "01bd2fq2-415b-3514-a61b-11052df2acbc");

            Assert.AreEqual(result.Message, "You cannot apply as a referee for this tournament because there is already registered referee.");
        }

        [Test]

        public async Task CheckForErrors_ShouldReturnNull()
        {
            var firstTournament = new Tournament()
            {
                Id = 12,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                CreatedOn = DateTime.Now,
                Description = "lkjhgfdvbjkiuytrdvbnjiuytfgbnjuytghjuytg",
                NumberOfTeams = 0,
                Status = FootballProjectSoftUni.Infrastructure.Data.Enums.TournamentStatus.Finished,
                OrganiserId = "600bafb9-a73d-4489-a387-643c3b8ae96c",
                RefereeId = null
            };

            _data.Tournaments.Add(firstTournament);

            _data.SaveChanges();

            var cityTournament = new TournamentCity()
            {
                CityId = 8,
                TournamentId = firstTournament.Id
            };

            _data.TournamentsCities.Add(cityTournament);

            _data.SaveChanges();

            var referee = new Referee()
            {
                Id = "01bq2fe2-415b-4514-a67b-11042df2acca",
                Birthdate = DateTime.Now.AddYears(-20),
                Name = "current",
                Experience = 5,
            };


            Assert.IsNull(await _refereeService.CheckForErrorsAsync(firstTournament.Id, referee.Id));

            
        }


        [Test]
        public async Task GetTournamets_ShouldBeCorrect()
        {
            var firstTournament = new Tournament()
            {
                Id = 13,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                CreatedOn = DateTime.Now,
                Description = "lkjhgfdvbjkiuytrdvbnjiuytfgbnjuytghjuytg",
                NumberOfTeams = 0,
                Status = FootballProjectSoftUni.Infrastructure.Data.Enums.TournamentStatus.Finished,
                OrganiserId = "600bafb9-a73d-4489-a387-643c3b8ae96c",
                RefereeId = null
            };

            _data.Tournaments.Add(firstTournament);

            _data.SaveChanges();

            var cityTournament = new TournamentCity()
            {
                CityId = 8,
                TournamentId = firstTournament.Id
            };

            _data.TournamentsCities.Add(cityTournament);

            _data.SaveChanges();

            var referee = new Referee()
            {
                Id = "01bq2fe2-415b-4514-a67b-51049df2acca",
                Birthdate = DateTime.Now.AddYears(-20),
                Name = "current",
                Experience = 5,
            };

            _data.Referees.Add(referee);

            _data.SaveChanges();

            firstTournament.Referee = referee;
            firstTournament.RefereeId = referee.Id;

            _data.SaveChanges();

            var tp = new TournamentParticipant()
            {
                TournamentId = firstTournament.Id,
                ParticipantId = referee.Id,
                Role = "Referee"
            };

            _data.TournamentsParticipants.Add(tp);

            _data.SaveChanges();

            var result = await _refereeService.GetTournamentsAsync(referee.Id);

            var model = new TournamentViewModel()
            {
                Id = 13,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                Status = FootballProjectSoftUni.Infrastructure.Data.Enums.TournamentStatus.Finished.ToString(),
                Description = "lkjhgfdvbjkiuytrdvbnjiuytfgbnjuytghjuytg",
                NumberOfTeams = 0,
                CityName = "Добрич",
                RefereeId = null
            };

            Assert.AreEqual(model.Id, firstTournament.Id);
            Assert.AreEqual(model.Description, firstTournament.Description);
            Assert.AreEqual(model.NumberOfTeams, firstTournament.NumberOfTeams);
            Assert.AreEqual(model.CityName, firstTournament.TournamentCities.Where(x => x.CityId == 8).Select(x => x.City.Name).FirstOrDefault());

        }

        [Test]
        public async Task CreateRefere_ShouldReturnFalse_ForTournament()
        {
            var model = new RefereeFormViewMOdel()
            {
                Id = 13,
                Name = "curr",
                Experience = 5,
                Birthdate = DateTime.Now.AddYears(-20).ToString(),

            };

            var result = await _refereeService.CreateRefereeToTournamentAsync(model, 77, "sdc", DateTime.Now);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task CreateReferee_ShouldBeCorrect()
        {
            var tournament = new Tournament()
            {
                Id = 14,
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

            var model = new RefereeFormViewMOdel()
            {
                Id = 15,
                Name = "murr",
                Experience = 5,
                Birthdate = DateTime.Now.AddYears(-20).ToString(),

            };

            var userId = "6df2e798-b555-402e-a374-0b359daedd31";

            var result = await _refereeService.CreateRefereeToTournamentAsync(model, tournament.Id, userId, DateTime.Now.AddYears(-20));

            Assert.IsTrue(result);
        }

        [Test]
        public async Task LevaeTournament_ShouldBeFalse_ForTP()
        {
            var result = await _refereeService.LeaveTournamentAsync(76, "wdsvcwe");

            Assert.IsFalse(result);
        }

        [Test]
        public async Task LevaeTournament_ShouldBeFalse_ForTournament()
        {
            var tp = new TournamentParticipant()
            {
                ParticipantId = "2dd70b49-a983-43f6-abc4-9f297e417b74",
                TournamentId = 64,
                Role = "Referee"
            };

            _data.TournamentsParticipants.Add(tp);

            _data.SaveChanges();

            var result = await _refereeService.LeaveTournamentAsync(64, "2dd70b49-a983-43f6-abc4-9f297e417b74");

            Assert.IsFalse(result);
        }

        [Test]
        public async Task LevaeTournament_ShouldBeTrue()
        {
            var tournament = new Tournament()
            {
                Id = 15,
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

            var tp = new TournamentParticipant()
            {
                ParticipantId = "2dd70b49-a983-4df6-abc4-9f297e417b74",
                TournamentId = 15,
                Role = "Referee"
            };

            _data.TournamentsParticipants.Add(tp);

            _data.SaveChanges();

            var referee = new Referee()
            {
                Id = "01bd2ve2-415b-4514-a6hb-11042df2acca",
                Birthdate = DateTime.Now.AddYears(-20),
                Name = "goshow",
                Experience = 5,
                Tournament = tournament,
                TournamentId = tournament.Id
            };

            _data.Referees.Add(referee);

            _data.SaveChanges();

            tournament.Referee = referee;
            tournament.RefereeId = referee.Id;
            _data.SaveChanges();

            var result = await _refereeService.LeaveTournamentAsync(15, "2dd70b49-a983-4df6-abc4-9f297e417b74");

            Assert.IsTrue(result);

            Assert.IsNull(tournament.RefereeId);
        }

        [Test]
        public async Task GetAllRefereesWithRatingsAsync_ReturnsCorrectData()
        {
            var referee1 = new Referee
            {
                Id = "r1",
                Name = "Ref One",
                Experience = 5,
                RefereedTournamentsCount = 3,
            };

            var referee2 = new Referee
            {
                Id = "r2",
                Name = "Ref Two",
                Experience = 2,
                RefereedTournamentsCount = 1,
            };

            _data.Referees.AddRange(referee1, referee2);
            await _data.SaveChangesAsync();

            var ratings = new List<RefereeRating>
            {
                new RefereeRating
                {
                    RefereeId = "r1",
                    UserId = "user1",
                    Value = 4
                },
                new RefereeRating
                {
                    RefereeId = "r1",
                    UserId = "user2",
                    Value = 5
                }
            };

            _data.RefereesRatings.AddRange(ratings);
            await _data.SaveChangesAsync();

            var result = (await _refereeService.GetAllRefereesWithRatingsAsync("any")).ToList();

            Assert.AreEqual(2, result.Count);

            var r1 = result.First(r => r.Id == "r1");
            Assert.AreEqual(4.5, r1.AverageRating);

            var r2 = result.First(r => r.Id == "r2");
            Assert.IsNull(r2.AverageRating);
        }

        [Test]
        public void RateRefereeAsync_WhenRatingIsOutOfRange_Throws()
        {
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _refereeService.RateRefereeAsync("r1", "u1", 0));

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _refereeService.RateRefereeAsync("r1", "u1", 6));
        }

        [Test]
        public void RateRefereeAsync_WhenRefereeDoesNotExist_ThrowsInvalidReferee()
        {
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _refereeService.RateRefereeAsync("missing", "u1", 5));

            Assert.AreEqual("Invalid referee.", ex!.Message);
        }

        [Test]
        public async Task RateRefereeAsync_WhenNoExistingRating_AddsNewRating()
        {
            var refereeId = "r1";
            var userId = "u1";

            _data.Referees.Add(new Referee
            {
                Id = refereeId,
                Name = "Ref",
                Experience = 1,
                RefereedTournamentsCount = 0
            });

            await _data.SaveChangesAsync();

            await _refereeService.RateRefereeAsync(refereeId, userId, 4);

            var saved = await _data.RefereesRatings
                .FirstOrDefaultAsync(x => x.RefereeId == refereeId && x.UserId == userId);

            Assert.IsNotNull(saved);
            Assert.AreEqual(4, saved!.Value);
        }

        [Test]
        public async Task RateRefereeAsync_WhenExistingRating_UpdatesValue()
        {
            var refereeId = "r1";
            var userId = "u1";

            _data.Referees.Add(new Referee
            {
                Id = refereeId,
                Name = "Ref",
                Experience = 1,
                RefereedTournamentsCount = 0
            });

            _data.RefereesRatings.Add(new RefereeRating
            {
                RefereeId = refereeId,
                UserId = userId,
                Value = 2
            });

            await _data.SaveChangesAsync();

            await _refereeService.RateRefereeAsync(refereeId, userId, 5);

            var saved = await _data.RefereesRatings
                .FirstAsync(x => x.RefereeId == refereeId && x.UserId == userId);

            Assert.AreEqual(5, saved.Value);

            var count = await _data.RefereesRatings
                .CountAsync(x => x.RefereeId == refereeId && x.UserId == userId);

            Assert.AreEqual(1, count);
        }

        [Test]
        public async Task AssignExistingRefereeToTournamentAsync_WhenRefereeMissing_ReturnsFalse()
        {
            _data.Tournaments.Add(new Tournament
            {
                Id = 1,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                CreatedOn = DateTime.UtcNow,
                Description = "test",
                NumberOfTeams = 0,
                Status = TournamentStatus.Upcoming,
                OrganiserId = "org"
            });
            await _data.SaveChangesAsync();

            var result = await _refereeService.AssignExistingRefereeToTournamentAsync("missing-ref", 1);

            Assert.AreEqual(false, result);
        }

        [Test]
        public async Task AssignExistingRefereeToTournamentAsync_WhenTournamentMissing_ReturnsFalse()
        {
            _data.Referees.Add(new Referee
            {
                Id = "r1",
                Name = "Ref",
                Experience = 1,
                RefereedTournamentsCount = 0
            });
            await _data.SaveChangesAsync();

            var result = await _refereeService.AssignExistingRefereeToTournamentAsync("r1", 999);

            Assert.AreEqual(false, result);
        }

        [Test]
        public async Task AssignExistingRefereeToTournamentAsync_WhenTournamentAlreadyHasReferee_ReturnsFalse()
        {
            // Arrange
            _data.Referees.Add(new Referee
            {
                Id = "r1",
                Name = "Ref1",
                Experience = 1,
                RefereedTournamentsCount = 0
            });

            _data.Tournaments.Add(new Tournament
            {
                Id = 1,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                CreatedOn = DateTime.UtcNow,
                Description = "test",
                NumberOfTeams = 0,
                Status = TournamentStatus.Upcoming,
                OrganiserId = "org",
                RefereeId = "someone-else" // вече има съдия
            });

            await _data.SaveChangesAsync();

            // Act
            var result = await _refereeService.AssignExistingRefereeToTournamentAsync("r1", 1);

            // Assert
            Assert.AreEqual(false, result);

            // и да не е увеличил броя турнири
            var r = await _data.Referees.FirstAsync(x => x.Id == "r1");
            Assert.AreEqual(0, r.RefereedTournamentsCount);
        }

        [Test]
        public async Task AssignExistingRefereeToTournamentAsync_WhenValid_AssignsReferee_IncrementsCount_AddsTp()
        {
            var userId = "r1";
            var tournamentId = 1;

            _data.Referees.Add(new Referee
            {
                Id = userId,
                Name = "Ref",
                Experience = 3,
                RefereedTournamentsCount = 7,
                TournamentId = null
            });

            _data.Tournaments.Add(new Tournament
            {
                Id = tournamentId,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                CreatedOn = DateTime.UtcNow,
                Description = "test",
                NumberOfTeams = 0,
                Status = TournamentStatus.Upcoming,
                OrganiserId = "org",
                RefereeId = null
            });

            await _data.SaveChangesAsync();

            var result = await _refereeService.AssignExistingRefereeToTournamentAsync(userId, tournamentId);

            Assert.AreEqual(true, result);

            var referee = await _data.Referees.FirstAsync(r => r.Id == userId);
            Assert.AreEqual(8, referee.RefereedTournamentsCount); // +1
            Assert.AreEqual(tournamentId, referee.TournamentId);

            var tournament = await _data.Tournaments.FirstAsync(t => t.Id == tournamentId);
            Assert.AreEqual(userId, tournament.RefereeId);

            var tpExists = await _data.TournamentsParticipants.AnyAsync(tp =>
                tp.ParticipantId == userId && tp.TournamentId == tournamentId && tp.Role == "Referee");

            Assert.IsTrue(tpExists);
        }

        [Test]
        public async Task GetRefereeByUserIdAsync_WhenRefereeExists_ReturnsReferee()
        {
            var referee = new Referee
            {
                Id = "r1",
                Name = "Ref",
                Experience = 5,
                RefereedTournamentsCount = 2
            };

            _data.Referees.Add(referee);
            await _data.SaveChangesAsync();

            var result = await _refereeService.GetRefereeByUserIdAsync("r1");

            Assert.IsNotNull(result);
            Assert.AreEqual("r1", result!.Id);
            Assert.AreEqual("Ref", result.Name);
        }

        [Test]
        public async Task GetRefereeByUserIdAsync_WhenRefereeDoesNotExist_ReturnsNull()
        {
            var result = await _refereeService.GetRefereeByUserIdAsync("missing");

            Assert.IsNull(result);
        }

        [Test]
        public void GetCommentsAsync_WhenRefereeDoesNotExist_Throws()
        {
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _refereeService.GetCommentsAsync("missing"));
        }

        [Test]
        public async Task GetCommentsAsync_ReturnsOrderedCommentsWithUserNames()
        {
            var refereeId = "r1";

            _data.Referees.Add(new Referee
            {
                Id = refereeId,
                Name = "Main Ref",
                Experience = 3,
                RefereedTournamentsCount = 1
            });

            _data.Users.AddRange(
                new ApplicationUser
                {
                    Id = "u1",
                    FirstName = "Ivan",
                    LastName = "Ivanov"
                },
                new ApplicationUser
                {
                    Id = "u2",
                    FirstName = "Petar",
                    LastName = "Petrov"
                });

            _data.RefereeComments.AddRange(
                new RefereeComment
                {
                    RefereeId = refereeId,
                    UserId = "u1",
                    Content = "First comment",
                    CreatedOn = DateTime.UtcNow.AddDays(-1)
                },
                new RefereeComment
                {
                    RefereeId = refereeId,
                    UserId = "u2",
                    Content = "Second comment",
                    CreatedOn = DateTime.UtcNow
                });

            await _data.SaveChangesAsync();

            var result = await _refereeService.GetCommentsAsync(refereeId);

            Assert.AreEqual(refereeId, result.RefereeId);
            Assert.AreEqual("Main Ref", result.RefereeName);

            Assert.AreEqual(2, result.Comments.Count);

            Assert.AreEqual("Second comment", result.Comments[0].Content);
            Assert.AreEqual("Petar Petrov", result.Comments[0].UserName);

            Assert.AreEqual("First comment", result.Comments[1].Content);
            Assert.AreEqual("Ivan Ivanov", result.Comments[1].UserName);
        }

        [Test]
        public async Task AddCommentAsync_AddsCommentToDatabase()
        {
            var refereeId = "r1";
            var userId = "u1";
            var content = "Great referee!";

            _data.Referees.Add(new Referee
            {
                Id = refereeId,
                Name = "Ref",
                Experience = 2,
                RefereedTournamentsCount = 0
            });

            _data.Users.Add(new ApplicationUser
            {
                Id = userId,
                FirstName = "Ivan",
                LastName = "Ivanov"
            });

            await _data.SaveChangesAsync();

            await _refereeService.AddCommentAsync(refereeId, userId, content);

            var savedComment = await _data.RefereeComments
                .FirstOrDefaultAsync(c => c.RefereeId == refereeId && c.UserId == userId);

            Assert.IsNotNull(savedComment);
            Assert.AreEqual(content, savedComment!.Content);
        }

        [Test]
        public async Task GetRefereeEmail_WhenUserExists_ReturnsEmail()
        {
            var userId = "u1";
            var email = "ref@test.com";

            _data.Users.Add(new ApplicationUser
            {
                Id = userId,
                Email = email,
                FirstName = "Ivan",
                LastName = "Ivanov"
            });

            await _data.SaveChangesAsync();

            var result = await _refereeService.GetRefereeEmail(userId);

            Assert.AreEqual(email, result);
        }

        [Test]
        public void GetRefereeEmail_WhenUserDoesNotExist_Throws()
        {
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _refereeService.GetRefereeEmail("missing-id"));
        }


    }
}
