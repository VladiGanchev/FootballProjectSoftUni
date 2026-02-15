using FootballProjectSoftUni.Core.Contracts.Team;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Models.City;
using FootballProjectSoftUni.Core.Models.Match;
using FootballProjectSoftUni.Core.Models.Tournament;
using FootballProjectSoftUni.Core.Services.Team;
using FootballProjectSoftUni.Core.Services.Tournament;
using FootballProjectSoftUni.Infrastructure.Data.Enums;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    public class TournamentServiceTests : UnitTestsBase
    {
        private ITournamentService _tournamentService;

        [SetUp]

        public void SetUp() => _tournamentService = new TournamentService(_data);

        private static Tournament CreateTournament(int id, string organiserId = "org-1", string? refereeId = null)
        {
            return new Tournament
            {
                Id = id,
                StartDate = DateTime.UtcNow.Date.AddDays(5),
                EndDate = DateTime.UtcNow.Date.AddDays(10),
                Description = "Test tournament description",
                OrganiserId = organiserId,
                NumberOfTeams = 8,
                CreatedOn = DateTime.UtcNow,
                RefereeId = refereeId,
                ImageUrl = "https://example.com/image.jpg",
                Status = TournamentStatus.Upcoming,
                Prize = 1000m,
                ParticipationFee = 10m,
                ReminderSent = false
            };
        }

        private static Tournament CreateTournament(
           int id,
           TournamentStatus status,
           DateTime start,
           DateTime end,
           int numberOfTeams = 8,
           string? refereeId = null,
           string desc = "Desc",
           string imageUrl = "https://example.com/t.jpg")
        {
            return new Tournament
            {
                Id = id,
                StartDate = start,
                EndDate = end,
                Description = desc,
                OrganiserId = "org-1",
                NumberOfTeams = numberOfTeams,
                CreatedOn = DateTime.UtcNow,
                RefereeId = refereeId,
                ImageUrl = imageUrl,
                Status = status,
                Prize = 100m,
                ParticipationFee = 10m
            };
        }

      
        
        [Test]
        public async Task DeleteTournamentAsync_ShouldReturnFalse_WhenTournamentDoesNotExist()
        {
            var result = await _tournamentService.DeleteTournamentAsync(99999);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task DeleteTournamentAsync_ShouldDeleteTournament_WhenExists_AndReturnTrue()
        {
            var tournament = CreateTournament(100);

            await _data.Tournaments.AddAsync(tournament);
            await _data.SaveChangesAsync();

            var result = await _tournamentService.DeleteTournamentAsync(100);

            Assert.IsTrue(result);
            Assert.IsFalse(await _data.Tournaments.AnyAsync(t => t.Id == 100));
        }

        [Test]
        public async Task DeleteTournamentAsync_ShouldNullRefereeTournamentId_AndTournamentRefereeId_WhenRefereeAttached()
        {
            var referee = new Referee
            {
                Id = "ref-1",
                Name = "Ref A",
                Birthdate = new DateTime(1980, 1, 1),
                Experience = 10,
                TournamentId = 101
            };

            var tournament = CreateTournament(101, refereeId: "ref-1");

            await _data.Referees.AddAsync(referee);
            await _data.Tournaments.AddAsync(tournament);
            await _data.SaveChangesAsync();

            var result = await _tournamentService.DeleteTournamentAsync(101);

            Assert.IsTrue(result);

            var refereeFromDb = await _data.Referees.FirstOrDefaultAsync(r => r.Id == "ref-1");
            Assert.NotNull(refereeFromDb);
            Assert.IsNull(refereeFromDb!.TournamentId);

            Assert.IsFalse(await _data.Tournaments.AnyAsync(t => t.Id == 101));
        }

        [Test]
        public async Task DeleteTournamentAsync_ShouldNotThrow_IfRefereeIdIsSetButRefereeMissing_AndStillDeletes()
        {
            var tournament = CreateTournament(107, refereeId: "missing-ref");

            await _data.Tournaments.AddAsync(tournament);
            await _data.SaveChangesAsync();

            var result = await _tournamentService.DeleteTournamentAsync(107);

            Assert.IsTrue(result);
            Assert.IsFalse(await _data.Tournaments.AnyAsync(t => t.Id == 107));
        }

        [Test]
        public async Task DeleteTournamentAsync_ShouldRemoveTournamentParticipants_WhenAny()
        {
            var tournament = CreateTournament(102);
            await _data.Tournaments.AddAsync(tournament);

            await _data.TournamentsParticipants.AddRangeAsync(
                new TournamentParticipant
                {
                    ParticipantId = "u1",
                    TournamentId = 102,
                    Role = "Player"
                },
                new TournamentParticipant
                {
                    ParticipantId = "u2",
                    TournamentId = 102,
                    Role = "Organiser"
                }
            );

            await _data.SaveChangesAsync();

            Assert.IsTrue(await _data.TournamentsParticipants.AnyAsync(tp => tp.TournamentId == 102));

            var result = await _tournamentService.DeleteTournamentAsync(102);

            Assert.IsTrue(result);
            Assert.IsFalse(await _data.TournamentsParticipants.AnyAsync(tp => tp.TournamentId == 102));
            Assert.IsFalse(await _data.Tournaments.AnyAsync(t => t.Id == 102));
        }

        [Test]
        public async Task DeleteTournamentAsync_ShouldRemoveTournamentCities_WhenAny()
        {
            var tournament = CreateTournament(105);
            await _data.Tournaments.AddAsync(tournament);

            await _data.TournamentsCities.AddRangeAsync(
                new TournamentCity
                {
                    TournamentId = 105,
                    CityId = 1
                },
                new TournamentCity
                {
                    TournamentId = 105,
                    CityId = 2
                }
            );

            await _data.SaveChangesAsync();

            Assert.IsTrue(await _data.TournamentsCities.AnyAsync(tc => tc.TournamentId == 105));

            var result = await _tournamentService.DeleteTournamentAsync(105);

            Assert.IsTrue(result);
            Assert.IsFalse(await _data.TournamentsCities.AnyAsync(tc => tc.TournamentId == 105));
            Assert.IsFalse(await _data.Tournaments.AnyAsync(t => t.Id == 105));
        }

        [Test]
        public async Task DeleteTournamentAsync_ShouldWork_WhenNoRelatedEntities_JustDeletesTournament()
        {
            var tournament = CreateTournament(106);
            await _data.Tournaments.AddAsync(tournament);
            await _data.SaveChangesAsync();

            var result = await _tournamentService.DeleteTournamentAsync(106);

            Assert.IsTrue(result);
            Assert.IsFalse(await _data.Tournaments.AnyAsync(t => t.Id == 106));
            Assert.IsFalse(await _data.TournamentsParticipants.AnyAsync(tp => tp.TournamentId == 106));
            Assert.IsFalse(await _data.TournamentsCities.AnyAsync(tc => tc.TournamentId == 106));
        }
        [Test]
        public void EditTournamentAsync_ShouldThrowArgumentException_WhenTournamentNotFound()
        {
            var model = new EditViewModel
            {
                Id = 9999,
                Description = "New desc",
                ImageUrl = "https://example.com/new.jpg",
                Winner = "Winner"
            };

            var start = new DateTime(2030, 1, 1);
            var end = new DateTime(2030, 1, 10);

            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _tournamentService.EditTournamentAsync(model, start, end));

            Assert.That(ex!.Message, Is.EqualTo("Invalid tournament ID"));
        }

        [Test]
        public async Task EditTournamentAsync_ShouldUpdateTournamentFields_WhenTournamentExists()
        {
            var tournament = CreateTournament(201);
            await _data.Tournaments.AddAsync(tournament);
            await _data.SaveChangesAsync();

            var model = new EditViewModel
            {
                Id = 201,
                Description = "New description",
                ImageUrl = "https://example.com/new.jpg",
                Winner = "Team A"
            };

            var newStart = new DateTime(2031, 5, 1);
            var newEnd = new DateTime(2031, 5, 20);

            await _tournamentService.EditTournamentAsync(model, newStart, newEnd);

            var updated = await _data.Tournaments.FirstAsync(t => t.Id == 201);

            Assert.That(updated.Description, Is.EqualTo("New description"));
            Assert.That(updated.ImageUrl, Is.EqualTo("https://example.com/new.jpg"));
            Assert.That(updated.Winner, Is.EqualTo("Team A"));
            Assert.That(updated.StartDate, Is.EqualTo(newStart));
            Assert.That(updated.EndDate, Is.EqualTo(newEnd));
        }

        [Test]
        public async Task FindCityAsync_ShouldReturnNull_WhenCityDoesNotExist()
        {
            CityViewModel result = await _tournamentService.FindCityAsync(99999);

            Assert.IsNull(result);
        }

        [Test]
        public async Task FindCityAsync_ShouldReturnCityViewModel_WhenCityExists()
        {
            var city = new City
            {
                Id = 500,
                Name = "Test City",
                ImageUrl = "https://example.com/city.jpg"
            };

            await _data.Cities.AddAsync(city);
            await _data.SaveChangesAsync();

            CityViewModel result = await _tournamentService.FindCityAsync(500);

            Assert.NotNull(result);
            Assert.That(result.Id, Is.EqualTo(500));
            Assert.That(result.Name, Is.EqualTo("Test City"));
            Assert.That(result.ImageUrl, Is.EqualTo("https://example.com/city.jpg"));
        }

        [Test]
        public async Task FindCityAsync_ShouldReturnCityViewModel_ForSeededCity_EvenIfImageUrlNullOrEmpty()
        {
            CityViewModel result = await _tournamentService.FindCityAsync(1);

            Assert.NotNull(result);
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.Name, Is.EqualTo("Благоевград"));
        }

        [Test]
        public async Task GetCitiesAsync_ShouldReturnAllSeededCities()
        {
            var result = await _tournamentService.GetCitiesAsync();
            var cities = result.ToList();

            Assert.That(cities.Count, Is.EqualTo(27)); 

            var blagoevgrad = cities.FirstOrDefault(c => c.Id == 1);
            Assert.NotNull(blagoevgrad);
            Assert.That(blagoevgrad!.Name, Is.EqualTo("Благоевград"));
        }

        [Test]
        public async Task GetCitiesAsync_ShouldMapCorrectly_Id_And_Name()
        {
            var city = new City
            {
                Id = 100,
                Name = "Test City",
                ImageUrl = "should not be mapped"
            };

            await _data.Cities.AddAsync(city);
            await _data.SaveChangesAsync();

            var result = await _tournamentService.GetCitiesAsync();
            var mapped = result.FirstOrDefault(c => c.Id == 100);

            Assert.NotNull(mapped);
            Assert.That(mapped!.Name, Is.EqualTo("Test City"));
        }

        [Test]
        public async Task GetCitiesAsync_ShouldReturnEmptyCollection_WhenNoCities()
        {
            _data.Cities.RemoveRange(_data.Cities);
            await _data.SaveChangesAsync();

            var result = await _tournamentService.GetCitiesAsync();

            Assert.IsEmpty(result);
        }

        [Test]
        public async Task GetCityTournamentsAsync_ShouldReturnNull_WhenCityDoesNotExist()
        {
            var result = await _tournamentService.GetCityTournamentsAsync(99999, showPast: false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetCityTournamentsAsync_ShouldReturnOnlyTournamentsForThatCity()
        {
            var t1 = CreateTournament(
                id: 301,
                TournamentStatus.Upcoming,
                DateTime.UtcNow.Date.AddDays(10),
                DateTime.UtcNow.Date.AddDays(12),
                refereeId: "ref-1",
                desc: "T1",
                imageUrl: "img1");

            var t2 = CreateTournament(
                id: 302,
                status: TournamentStatus.Upcoming,
                start: DateTime.UtcNow.Date.AddDays(20),
                end: DateTime.UtcNow.Date.AddDays(22),
                desc: "T2",
                imageUrl: "img2");

            var otherCityTournament = CreateTournament(
                id: 303,
                status: TournamentStatus.Upcoming,
                start: DateTime.UtcNow.Date.AddDays(30),
                end: DateTime.UtcNow.Date.AddDays(31),
                desc: "OTHER",
                imageUrl: "img3");

            await _data.Tournaments.AddRangeAsync(t1, t2, otherCityTournament);

            await _data.TournamentsCities.AddRangeAsync(
                new TournamentCity { TournamentId = 301, CityId = 1 },
                new TournamentCity { TournamentId = 302, CityId = 1 },
                new TournamentCity { TournamentId = 303, CityId = 2 } 
            );

            await _data.SaveChangesAsync();

            var result = (await _tournamentService.GetCityTournamentsAsync(1, showPast: false))!.ToList();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Any(x => x.Id == 301), Is.True);
            Assert.That(result.Any(x => x.Id == 302), Is.True);
            Assert.That(result.Any(x => x.Id == 303), Is.False);

            var mapped = result.First(x => x.Id == 301);
            Assert.That(mapped.Description, Is.EqualTo("T1"));
            Assert.That(mapped.RefereeId, Is.EqualTo("ref-1"));
            Assert.That(mapped.NumberOfTeams, Is.EqualTo(8));
            Assert.That(mapped.ImageUrl, Is.EqualTo("img1"));
            Assert.That(mapped.StartDate, Is.EqualTo(t1.StartDate));
            Assert.That(mapped.EndDate, Is.EqualTo(t1.EndDate));
            Assert.That(mapped.Status, Is.EqualTo(t1.Status.ToString()));
        }

        [Test]
        public async Task GetCityTournamentsAsync_ShowPastTrue_ShouldReturnOnlyFinished_OrderedByEndDateDesc()
        {
            var finishedOld = CreateTournament(
                id: 401,
                status: TournamentStatus.Finished,
                start: DateTime.UtcNow.Date.AddDays(-20),
                end: DateTime.UtcNow.Date.AddDays(-15),
                desc: "Finished Old");

            var finishedNew = CreateTournament(
                id: 402,
                status: TournamentStatus.Finished,
                start: DateTime.UtcNow.Date.AddDays(-10),
                end: DateTime.UtcNow.Date.AddDays(-1),
                desc: "Finished New");

            var upcoming = CreateTournament(
                id: 403,
                status: TournamentStatus.Upcoming,
                start: DateTime.UtcNow.Date.AddDays(5),
                end: DateTime.UtcNow.Date.AddDays(8),
                desc: "Upcoming");

            await _data.Tournaments.AddRangeAsync(finishedOld, finishedNew, upcoming);

            await _data.TournamentsCities.AddRangeAsync(
                new TournamentCity { TournamentId = 401, CityId = 1 },
                new TournamentCity { TournamentId = 402, CityId = 1 },
                new TournamentCity { TournamentId = 403, CityId = 1 }
            );

            await _data.SaveChangesAsync();

            var result = (await _tournamentService.GetCityTournamentsAsync(1, showPast: true))!.ToList();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.All(x => x.Status == TournamentStatus.Finished.ToString()), Is.True);

            Assert.That(result[0].Id, Is.EqualTo(402));
            Assert.That(result[1].Id, Is.EqualTo(401));
        }

        [Test]
        public async Task GetCityTournamentsAsync_ShowPastFalse_ShouldExcludeFinished()
        {
            var finished = CreateTournament(
                id: 501,
                status: TournamentStatus.Finished,
                start: DateTime.UtcNow.Date.AddDays(-5),
                end: DateTime.UtcNow.Date.AddDays(-1));

            var upcoming = CreateTournament(
                id: 502,
                status: TournamentStatus.Upcoming,
                start: DateTime.UtcNow.Date.AddDays(2),
                end: DateTime.UtcNow.Date.AddDays(4));

            await _data.Tournaments.AddRangeAsync(finished, upcoming);

            await _data.TournamentsCities.AddRangeAsync(
                new TournamentCity { TournamentId = 501, CityId = 1 },
                new TournamentCity { TournamentId = 502, CityId = 1 }
            );

            await _data.SaveChangesAsync();

            var result = (await _tournamentService.GetCityTournamentsAsync(1, showPast: false))!.ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(502));
            Assert.That(result[0].Status, Is.Not.EqualTo(TournamentStatus.Finished.ToString()));
        }

        [Test]
        public async Task GetCityTournamentsAsync_ShouldReturnEmpty_WhenCityExistsButNoTournaments()
        {
            var result = (await _tournamentService.GetCityTournamentsAsync(1, showPast: false))!.ToList();

            Assert.IsEmpty(result);
        }

        [Test]
        public async Task GetTournamentDetailsAsync_ShouldReturnNull_WhenTournamentNotFound()
        {
            var result = await _tournamentService.GetTournamentDetailsAsync(99999);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetTournamentDetailsAsync_ShouldMapDetailsCorrectly_WhenValidTournament()
        {
            var referee = new Referee
            {
                Id = "ref-1",
                Name = "Ref Name",
                Birthdate = new DateTime(1980, 1, 1),
                Experience = 10,
                TournamentId = 602
            };

            var tournament = CreateTournament(602, refereeId: "ref-1");

            await _data.Referees.AddAsync(referee);
            await _data.Tournaments.AddAsync(tournament);

            await _data.TournamentsCities.AddAsync(new TournamentCity
            {
                TournamentId = 602,
                CityId = 1
            });

            await _data.SaveChangesAsync();

            var result = await _tournamentService.GetTournamentDetailsAsync(602);

            Assert.NotNull(result);

            Assert.That(result!.Id, Is.EqualTo(602));
            Assert.That(result.Description, Is.EqualTo("Test tournament description"));
            Assert.That(result.RefereeId, Is.EqualTo("ref-1"));
            Assert.That(result.RefereeName, Is.EqualTo("Ref Name"));
            Assert.That(result.Status, Is.EqualTo(TournamentStatus.Upcoming.ToString()));
            Assert.That(result.NumberOfTeams, Is.EqualTo(8));
            Assert.That(result.CityId, Is.EqualTo(1));
            Assert.That(result.Prize, Is.EqualTo(1000m));
            Assert.That(result.ParticipationFee, Is.EqualTo(10m));
        }

        [Test]
        public void GetTournamentDetailsAsync_ShouldThrowNullReference_WhenTournamentHasNoCities()
        {
            var tournament = CreateTournament(601);
            _data.Tournaments.Add(tournament);
            _data.SaveChanges();

            Assert.ThrowsAsync<NullReferenceException>(async () =>
                await _tournamentService.GetTournamentDetailsAsync(601));
        }


        [Test]
        public async Task GetTournamentDetailsAsync_ShouldSortMatchesByRoundThenIndexInRound_AndMapNamesWithFallbacks()
        {
            var tournament = CreateTournament(603);
            await _data.Tournaments.AddAsync(tournament);

            await _data.TournamentsCities.AddAsync(new TournamentCity
            {
                TournamentId = 603,
                CityId = 1
            });

            var team1 = new Team { Id = 2001, Name = "Alpha" };
            var team2 = new Team { Id = 2002, Name = "Beta" };
            var winner = new Team { Id = 2003, Name = "WinnerX" };
            await _data.Teams.AddRangeAsync(team1, team2, winner);

            await _data.Matches.AddRangeAsync(
                new Match
                {
                    Id = 9001,
                    TournamentId = 603,
                    Round = 2,
                    IndexInRound = 2,
                    Team1Id = 2001,
                    Team2Id = 2002,
                    Team1Goals = 1,
                    Team2Goals = 0,
                    WinnerTeamId = 2001
                },
                new Match
                {
                    Id = 9002,
                    TournamentId = 603,
                    Round = 1,
                    IndexInRound = 2,
                    Team1Id = null,    
                    Team2Id = 2002,
                    Team1Goals = 0,
                    Team2Goals = 3,
                    WinnerTeamId = null 
                },
                new Match
                {
                    Id = 9003,
                    TournamentId = 603,
                    Round = 1,
                    IndexInRound = 1,
                    Team1Id = 2001,
                    Team2Id = null,   
                    Team1Goals = 2,
                    Team2Goals = 2,
                    WinnerTeamId = 2003
                }
            );

            await _data.SaveChangesAsync();

            var result = await _tournamentService.GetTournamentDetailsAsync(603);

            Assert.NotNull(result);
            Assert.NotNull(result!.Matches);
            Assert.That(result.Matches, Has.Count.EqualTo(3));

            Assert.That(result.Matches[0].Id, Is.EqualTo(9003));
            Assert.That(result.Matches[1].Id, Is.EqualTo(9002));
            Assert.That(result.Matches[2].Id, Is.EqualTo(9001));

            var m1 = result.Matches[0];
            Assert.That(m1.Team1Name, Is.EqualTo("Alpha"));
            Assert.That(m1.Team2Name, Is.EqualTo("---"));
            Assert.That(m1.WinnerTeamName, Is.EqualTo("WinnerX"));

            var m2 = result.Matches[1]; 
            Assert.That(m2.Team1Name, Is.EqualTo("---"));
            Assert.That(m2.Team2Name, Is.EqualTo("Beta"));
            Assert.That(m2.WinnerTeamName, Is.Null);
        }

        [Test]
        public async Task AddTournamentToCityAsync_ShouldCreateTournament_AndLinkToCity()
        {
            int cityId = 1; 
            var start = new DateTime(2032, 6, 1);
            var end = new DateTime(2032, 6, 10);

            var model = new AddTournamentFormViewModel
            {
                Description = "My new tournament",
                ImageUrl = "https://example.com/new.jpg",
                Prize = 1234m,
                ParticipationFee = 55m
            };

            var tournamentsBefore = await _data.Tournaments.CountAsync();
            var linksBefore = await _data.TournamentsCities.CountAsync();

            await _tournamentService.AddTournamentToCityAsync(model, cityId, start, end);

            var tournamentsAfter = await _data.Tournaments.CountAsync();
            Assert.That(tournamentsAfter, Is.EqualTo(tournamentsBefore + 1));

            var createdTournament = await _data.Tournaments
                .OrderByDescending(t => t.Id)
                .FirstAsync();

            Assert.That(createdTournament.StartDate, Is.EqualTo(start));
            Assert.That(createdTournament.EndDate, Is.EqualTo(end));
            Assert.That(createdTournament.Description, Is.EqualTo("My new tournament"));
            Assert.That(createdTournament.ImageUrl, Is.EqualTo("https://example.com/new.jpg"));
            Assert.That(createdTournament.Prize, Is.EqualTo(1234m));
            Assert.That(createdTournament.ParticipationFee, Is.EqualTo(55m));

            Assert.That(createdTournament.NumberOfTeams, Is.EqualTo(0));
            Assert.That(createdTournament.Status, Is.EqualTo(TournamentStatus.Upcoming));
            Assert.That(createdTournament.RefereeId, Is.Null);
            Assert.That(createdTournament.OrganiserId, Is.EqualTo("600bafb9-a73d-4489-a387-643c2b8ae96c"));

            var linksAfter = await _data.TournamentsCities.CountAsync();
            Assert.That(linksAfter, Is.EqualTo(linksBefore + 1));

            var link = await _data.TournamentsCities
                .FirstOrDefaultAsync(x => x.TournamentId == createdTournament.Id && x.CityId == cityId);

            Assert.NotNull(link);
        }

        [Test]
        public async Task AddTournamentToCityAsync_ShouldIncreaseAppStatsTournamentsCreatedTotal_WhenStatsExist()
        {
            var stats = await _data.AppStats.FindAsync(1);
            if (stats == null)
            {
                stats = new TournamentStats { Id = 1, TournamentsCreatedTotal = 5 };
                await _data.AppStats.AddAsync(stats);
                await _data.SaveChangesAsync();
            }

            int before = stats.TournamentsCreatedTotal;

            var model = new AddTournamentFormViewModel
            {
                Description = "Stats tournament",
                ImageUrl = "img",
                Prize = 10m,
                ParticipationFee = 1m
            };

            await _tournamentService.AddTournamentToCityAsync(model, cityId: 1,
                start: new DateTime(2033, 1, 1),
                end: new DateTime(2033, 1, 2));

            var afterStats = await _data.AppStats.FindAsync(1);
            Assert.NotNull(afterStats);
            Assert.That(afterStats!.TournamentsCreatedTotal, Is.EqualTo(before + 1));
        }

        [Test]
        public async Task AddTournamentToCityAsync_ShouldNotThrow_WhenAppStatsMissing()
        {
            var existing = await _data.AppStats.FindAsync(1);
            if (existing != null)
            {
                _data.AppStats.Remove(existing);
                await _data.SaveChangesAsync();
            }

            var model = new AddTournamentFormViewModel
            {
                Description = "No stats tournament",
                ImageUrl = "img",
                Prize = 10m,
                ParticipationFee = 1m
            };

            Assert.DoesNotThrowAsync(async () =>
                await _tournamentService.AddTournamentToCityAsync(model, cityId: 1,
                    start: new DateTime(2034, 1, 1),
                    end: new DateTime(2034, 1, 2)));
        }

        [Test]
        public async Task FindTournamentAsync_ShouldReturnNull_WhenTournamentDoesNotExist()
        {
            var result = await _tournamentService.FindTournamentAsync(99999);

            Assert.IsNull(result);
        }

        [Test]
        public async Task FindTournamentAsync_ShouldMapEditViewModelCorrectly_WhenTournamentExists()
        {
            var tournament = CreateTournament(701);
            tournament.Description = "Desc 701";
            tournament.ImageUrl = "https://example.com/701.jpg";
            tournament.Winner = "Team X";

            tournament.StartDate = new DateTime(2035, 3, 1, 14, 30, 0);
            tournament.EndDate = new DateTime(2035, 3, 10, 18, 0, 0);
            tournament.CreatedOn = new DateTime(2035, 2, 20, 9, 15, 0);

            await _data.Tournaments.AddAsync(tournament);
            await _data.TournamentsCities.AddAsync(new TournamentCity
            {
                TournamentId = 701,
                CityId = 1
            });
            await _data.SaveChangesAsync();

            var result = await _tournamentService.FindTournamentAsync(701);

            Assert.NotNull(result);
            Assert.That(result!.Id, Is.EqualTo(701));
            Assert.That(result.Description, Is.EqualTo("Desc 701"));
            Assert.That(result.ImageUrl, Is.EqualTo("https://example.com/701.jpg"));
            Assert.That(result.Winner, Is.EqualTo("Team X"));

            Assert.That(result.StartDate, Is.EqualTo(tournament.StartDate.ToString("dd/MM/yyyy HH:mm")));
            Assert.That(result.EndDate, Is.EqualTo(tournament.EndDate.ToString("dd/MM/yyyy HH:mm")));
            Assert.That(result.CreatedOn, Is.EqualTo(tournament.CreatedOn.ToString("dd/MM/yyyy HH:mm")));
        }

        [Test]
        public async Task FindTournamentByIdAsync_ShouldReturnNull_WhenTournamentDoesNotExist()
        {
            var result = await _tournamentService.FindTournamentByIdAsync(99999);

            Assert.IsNull(result);
        }

        [Test]
        public async Task FindTournamentByIdAsync_ShouldReturnTournament_WithIncludedCities_WhenExists()
        {
            var tournament = CreateTournament(801);
            await _data.Tournaments.AddAsync(tournament);

            await _data.TournamentsCities.AddAsync(new TournamentCity
            {
                TournamentId = 801,
                CityId = 1
            });

            await _data.SaveChangesAsync();

            var result = await _tournamentService.FindTournamentByIdAsync(801);

            Assert.NotNull(result);
            Assert.That(result!.Id, Is.EqualTo(801));
            Assert.That(result.Description, Is.EqualTo("Test tournament description"));

            Assert.NotNull(result.TournamentCities);
            Assert.That(result.TournamentCities.Count(), Is.EqualTo(1));
            Assert.That(result.TournamentCities.First().CityId, Is.EqualTo(1));
            Assert.NotNull(result.TournamentCities.First().City);
        }

        [Test]
        public void GenerateBracketAsync_ShouldThrowArgumentException_WhenTournamentDoesNotExist()
        {
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _tournamentService.GenerateBracketAsync(99999));
        }

        [Test]
        public async Task GenerateBracketAsync_ShouldCreate15Matches_WhenNoMatchesExist()
        {
            var tournament = CreateTournament(901);
            await _data.Tournaments.AddAsync(tournament);
            await _data.SaveChangesAsync();

            await _tournamentService.GenerateBracketAsync(901);

            var matches = await _data.Matches
                .Where(m => m.TournamentId == 901)
                .ToListAsync();

            Assert.That(matches.Count, Is.EqualTo(15));

            Assert.That(matches.Count(m => m.Round == 1), Is.EqualTo(8));
            Assert.That(matches.Count(m => m.Round == 2), Is.EqualTo(4));
            Assert.That(matches.Count(m => m.Round == 3), Is.EqualTo(2));
            Assert.That(matches.Count(m => m.Round == 4), Is.EqualTo(1));

            Assert.That(matches.Where(m => m.Round == 1).Select(m => m.IndexInRound).OrderBy(x => x),
                Is.EqualTo(Enumerable.Range(0, 8)));

            Assert.That(matches.Where(m => m.Round == 2).Select(m => m.IndexInRound).OrderBy(x => x),
                Is.EqualTo(Enumerable.Range(0, 4)));

            Assert.That(matches.Where(m => m.Round == 3).Select(m => m.IndexInRound).OrderBy(x => x),
                Is.EqualTo(Enumerable.Range(0, 2)));

            Assert.That(matches.Single(m => m.Round == 4).IndexInRound, Is.EqualTo(0));
        }

        [Test]
        public async Task GenerateBracketAsync_ShouldNotCreateMatches_WhenMatchesAlreadyExist()
        {
            var tournament = CreateTournament(902);
            await _data.Tournaments.AddAsync(tournament);

            await _data.Matches.AddAsync(new Match
            {
                TournamentId = 902,
                Round = 1,
                IndexInRound = 0
            });

            await _data.SaveChangesAsync();

            await _tournamentService.GenerateBracketAsync(902);

            var matches = await _data.Matches
                .Where(m => m.TournamentId == 902)
                .ToListAsync();

            Assert.That(matches.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task AssignTeamToBracketAsync_ShouldAssignToFirstFreeTeam1Slot()
        {
            int tournamentId = 1001;

            await _data.Matches.AddRangeAsync(
                new Match { TournamentId = tournamentId, Round = 1, IndexInRound = 0, Team1Id = null, Team2Id = null },
                new Match { TournamentId = tournamentId, Round = 1, IndexInRound = 1, Team1Id = null, Team2Id = null }
            );
            await _data.SaveChangesAsync();

            await _tournamentService.AssignTeamToBracketAsync(tournamentId, teamId: 55);

            var match0 = await _data.Matches.FirstAsync(m => m.TournamentId == tournamentId && m.Round == 1 && m.IndexInRound == 0);
            Assert.That(match0.Team1Id, Is.EqualTo(55));
            Assert.That(match0.Team2Id, Is.Null);
        }

        [Test]
        public async Task AssignTeamToBracketAsync_ShouldAssignToFirstFreeTeam2Slot_WhenTeam1AlreadySet()
        {
            int tournamentId = 1002;

            await _data.Matches.AddRangeAsync(
                new Match { TournamentId = tournamentId, Round = 1, IndexInRound = 0, Team1Id = 11, Team2Id = null },
                new Match { TournamentId = tournamentId, Round = 1, IndexInRound = 1, Team1Id = null, Team2Id = null }
            );
            await _data.SaveChangesAsync();

            await _tournamentService.AssignTeamToBracketAsync(tournamentId, teamId: 66);

            var match0 = await _data.Matches.FirstAsync(m => m.TournamentId == tournamentId && m.Round == 1 && m.IndexInRound == 0);
            Assert.That(match0.Team1Id, Is.EqualTo(11));
            Assert.That(match0.Team2Id, Is.EqualTo(66));
        }

        [Test]
        public async Task AssignTeamToBracketAsync_ShouldFillMatchesInIndexOrder()
        {
            int tournamentId = 1003;

            await _data.Matches.AddRangeAsync(
                new Match { TournamentId = tournamentId, Round = 1, IndexInRound = 0, Team1Id = 1, Team2Id = 2 },   
                new Match { TournamentId = tournamentId, Round = 1, IndexInRound = 1, Team1Id = null, Team2Id = null } 
            );
            await _data.SaveChangesAsync();

            await _tournamentService.AssignTeamToBracketAsync(tournamentId, teamId: 77);

            var match1 = await _data.Matches.FirstAsync(m => m.TournamentId == tournamentId && m.Round == 1 && m.IndexInRound == 1);
            Assert.That(match1.Team1Id, Is.EqualTo(77));
            Assert.That(match1.Team2Id, Is.Null);
        }

        [Test]
        public void AssignTeamToBracketAsync_ShouldThrowInvalidOperationException_WhenNoFreeSlots()
        {
            int tournamentId = 1004;

            _data.Matches.AddRange(
                new Match { TournamentId = tournamentId, Round = 1, IndexInRound = 0, Team1Id = 1, Team2Id = 2 },
                new Match { TournamentId = tournamentId, Round = 1, IndexInRound = 1, Team1Id = 3, Team2Id = 4 }
            );
            _data.SaveChanges();

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _tournamentService.AssignTeamToBracketAsync(tournamentId, teamId: 88));
        }

        [Test]
        public void AssignTeamToBracketAsync_ShouldThrowInvalidOperationException_WhenNoFirstRoundMatches()
        {
            int tournamentId = 1005; 

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _tournamentService.AssignTeamToBracketAsync(tournamentId, teamId: 99));
        }

        [Test]
        public async Task MoveWinnerToNextRoundAsync_ShouldReturn_WhenMatchNotFound()
        {
            await _tournamentService.MoveWinnerToNextRoundAsync(99999);
            Assert.Pass();
        }

        [Test]
        public async Task MoveWinnerToNextRoundAsync_ShouldReturn_WhenWinnerTeamIdIsNull()
        {
            var m = new Match { Id = 1101, TournamentId = 2001, Round = 1, IndexInRound = 0, WinnerTeamId = null };
            await _data.Matches.AddAsync(m);
            await _data.SaveChangesAsync();

            await _tournamentService.MoveWinnerToNextRoundAsync(1101);

            var reloaded = await _data.Matches.FirstAsync(x => x.Id == 1101);
            Assert.IsNull(reloaded.WinnerTeamId);
        }

        [Test]
        public async Task MoveWinnerToNextRoundAsync_ShouldMoveWinnerToNextMatch_Team1_WhenIndexEven()
        {
            int tournamentId = 2002;

            await _data.Matches.AddRangeAsync(
                new Match { Id = 1201, TournamentId = tournamentId, Round = 1, IndexInRound = 0, WinnerTeamId = 55 },
                new Match { Id = 1202, TournamentId = tournamentId, Round = 2, IndexInRound = 0, Team1Id = null, Team2Id = null }
            );
            await _data.SaveChangesAsync();

            await _tournamentService.MoveWinnerToNextRoundAsync(1201);

            var next = await _data.Matches.FirstAsync(m => m.Id == 1202);
            Assert.That(next.Team1Id, Is.EqualTo(55));
            Assert.IsNull(next.Team2Id);
        }

        [Test]
        public async Task MoveWinnerToNextRoundAsync_ShouldMoveWinnerToNextMatch_Team2_WhenIndexOdd()
        {
            int tournamentId = 2003;

            await _data.Matches.AddRangeAsync(
                new Match { Id = 1301, TournamentId = tournamentId, Round = 1, IndexInRound = 1, WinnerTeamId = 66 },
                new Match { Id = 1302, TournamentId = tournamentId, Round = 2, IndexInRound = 0, Team1Id = null, Team2Id = null }
            );
            await _data.SaveChangesAsync();

            await _tournamentService.MoveWinnerToNextRoundAsync(1301);

            var next = await _data.Matches.FirstAsync(m => m.Id == 1302);
            Assert.IsNull(next.Team1Id);
            Assert.That(next.Team2Id, Is.EqualTo(66));
        }

        [Test]
        public async Task MoveWinnerToNextRoundAsync_ShouldReturn_WhenNextMatchNotFound()
        {
            int tournamentId = 2004;

            await _data.Matches.AddAsync(
                new Match { Id = 1401, TournamentId = tournamentId, Round = 1, IndexInRound = 0, WinnerTeamId = 77 }
            );
            await _data.SaveChangesAsync();

            await _tournamentService.MoveWinnerToNextRoundAsync(1401);

            var reloaded = await _data.Matches.FirstAsync(m => m.Id == 1401);
            Assert.That(reloaded.WinnerTeamId, Is.EqualTo(77));
        }

        [Test]
        public async Task MoveWinnerToNextRoundAsync_WhenFinalRound_ShouldSetTournamentWinnerName()
        {
            int tournamentId = 2005;

            var tournament = CreateTournament(tournamentId);
            tournament.Winner = null;
            await _data.Tournaments.AddAsync(tournament);

            var team = new Team { Id = 5001, Name = "CHAMP" };
            await _data.Teams.AddAsync(team);

            await _data.Matches.AddAsync(new Match
            {
                Id = 1501,
                TournamentId = tournamentId,
                Round = 4,
                IndexInRound = 0,
                WinnerTeamId = 5001
            });

            await _data.SaveChangesAsync();

            await _tournamentService.MoveWinnerToNextRoundAsync(1501);

            var updatedTournament = await _data.Tournaments.FirstAsync(t => t.Id == tournamentId);
            Assert.That(updatedTournament.Winner, Is.EqualTo("CHAMP"));
        }

        [Test]
        public async Task MoveWinnerToNextRoundAsync_WhenFinalRound_ShouldReturnEvenIfTournamentMissing()
        {
            var team = new Team { Id = 5002, Name = "CHAMP2" };
            await _data.Teams.AddAsync(team);

            await _data.Matches.AddAsync(new Match
            {
                Id = 1601,
                TournamentId = 999001,
                Round = 4,
                IndexInRound = 0,
                WinnerTeamId = 5002
            });

            await _data.SaveChangesAsync();

            Assert.DoesNotThrowAsync(async () =>
                await _tournamentService.MoveWinnerToNextRoundAsync(1601));
        }

        [Test]
        public async Task RemoveTeamFromBracketAsync_ShouldReturn_WhenTournamentNotFound()
        {
            Assert.DoesNotThrowAsync(async () =>
                await _tournamentService.RemoveTeamFromBracketAsync(99999, teamId: 10));
        }

        [Test]
        public async Task RemoveTeamFromBracketAsync_ShouldNotChangeAnything_WhenTournamentAlreadyStartedOrToday()
        {
            int tournamentId = 3001;

            var tournament = CreateTournament(tournamentId);
            tournament.StartDate = DateTime.Now.AddMinutes(-1); 
            await _data.Tournaments.AddAsync(tournament);

            await _data.Matches.AddRangeAsync(
                new Match
                {
                    Id = 7001,
                    TournamentId = tournamentId,
                    Round = 1,
                    IndexInRound = 0,
                    Team1Id = 10,
                    Team2Id = 11,
                    WinnerTeamId = 10,
                    Team1Goals = 2,
                    Team2Goals = 1
                }
            );

            await _data.SaveChangesAsync();

            await _tournamentService.RemoveTeamFromBracketAsync(tournamentId, teamId: 10);

            var match = await _data.Matches.FirstAsync(m => m.Id == 7001);
            Assert.That(match.Team1Id, Is.EqualTo(10));
            Assert.That(match.Team2Id, Is.EqualTo(11));
            Assert.That(match.WinnerTeamId, Is.EqualTo(10));
            Assert.That(match.Team1Goals, Is.EqualTo(2));
            Assert.That(match.Team2Goals, Is.EqualTo(1));
        }

        [Test]
        public async Task RemoveTeamFromBracketAsync_ShouldNullOutTeamSlotsAndWinner_AndResetGoals_WhenTournamentNotStarted()
        {
            int tournamentId = 3002;

            var tournament = CreateTournament(tournamentId);
            tournament.StartDate = DateTime.Now.AddDays(2); 
            await _data.Tournaments.AddAsync(tournament);

            await _data.Matches.AddRangeAsync(
                new Match
                {
                    Id = 7101,
                    TournamentId = tournamentId,
                    Round = 1,
                    IndexInRound = 0,
                    Team1Id = 10,
                    Team2Id = 11,
                    WinnerTeamId = 10,
                    Team1Goals = 3,
                    Team2Goals = 2
                },
                new Match
                {
                    Id = 7102,
                    TournamentId = tournamentId,
                    Round = 1,
                    IndexInRound = 1,
                    Team1Id = 12,
                    Team2Id = 10,          
                    WinnerTeamId = null,
                    Team1Goals = 1,
                    Team2Goals = 1
                },
                new Match
                {
                    Id = 7103,
                    TournamentId = tournamentId,
                    Round = 2,
                    IndexInRound = 0,
                    Team1Id = 10,          
                    Team2Id = null,
                    WinnerTeamId = 10,     
                    Team1Goals = 5,
                    Team2Goals = 0
                }
            );

            await _data.SaveChangesAsync();

            await _tournamentService.RemoveTeamFromBracketAsync(tournamentId, teamId: 10);

            var m1 = await _data.Matches.FirstAsync(m => m.Id == 7101);
            Assert.IsNull(m1.Team1Id);
            Assert.That(m1.Team2Id, Is.EqualTo(11));
            Assert.IsNull(m1.WinnerTeamId);
            Assert.IsNull(m1.Team1Goals);
            Assert.IsNull(m1.Team2Goals);

            var m2 = await _data.Matches.FirstAsync(m => m.Id == 7102);
            Assert.That(m2.Team1Id, Is.EqualTo(12));
            Assert.IsNull(m2.Team2Id);
            Assert.IsNull(m2.WinnerTeamId);
            Assert.IsNull(m2.Team1Goals);
            Assert.IsNull(m2.Team2Goals);

            var m3 = await _data.Matches.FirstAsync(m => m.Id == 7103);
            Assert.IsNull(m3.Team1Id);
            Assert.IsNull(m3.Team2Id);
            Assert.IsNull(m3.WinnerTeamId);
            Assert.IsNull(m3.Team1Goals);
            Assert.IsNull(m3.Team2Goals);
        }

        [Test]
        public async Task RemoveTeamFromBracketAsync_ShouldOnlyAffectMatchesThatContainThatTeam()
        {
            int tournamentId = 3003;

            var tournament = CreateTournament(tournamentId);
            tournament.StartDate = DateTime.Now.AddDays(3);
            await _data.Tournaments.AddAsync(tournament);

            await _data.Matches.AddRangeAsync(
                new Match
                {
                    Id = 7201,
                    TournamentId = tournamentId,
                    Round = 1,
                    IndexInRound = 0,
                    Team1Id = 10,
                    Team2Id = 11,
                    WinnerTeamId = 10,
                    Team1Goals = 2,
                    Team2Goals = 1
                },
                new Match
                {
                    Id = 7202,
                    TournamentId = tournamentId,
                    Round = 1,
                    IndexInRound = 1,
                    Team1Id = 20, 
                    Team2Id = 21,
                    WinnerTeamId = 20,
                    Team1Goals = 4,
                    Team2Goals = 0
                }
            );

            await _data.SaveChangesAsync();

            await _tournamentService.RemoveTeamFromBracketAsync(tournamentId, teamId: 10);

            var affected = await _data.Matches.FirstAsync(m => m.Id == 7201);
            Assert.IsNull(affected.Team1Id);
            Assert.That(affected.Team2Id, Is.EqualTo(11));
            Assert.IsNull(affected.WinnerTeamId);
            Assert.IsNull(affected.Team1Goals);
            Assert.IsNull(affected.Team2Goals);

            var untouched = await _data.Matches.FirstAsync(m => m.Id == 7202);
            Assert.That(untouched.Team1Id, Is.EqualTo(20));
            Assert.That(untouched.Team2Id, Is.EqualTo(21));
            Assert.That(untouched.WinnerTeamId, Is.EqualTo(20));
            Assert.That(untouched.Team1Goals, Is.EqualTo(4));
            Assert.That(untouched.Team2Goals, Is.EqualTo(0));
        }

        [Test]
        public async Task GetEnterResultModelAsync_ShouldReturnNull_WhenMatchNotFound()
        {
            var result = await _tournamentService.GetEnterResultModelAsync(99999);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetEnterResultModelAsync_ShouldMapAllFieldsCorrectly_WhenMatchExists()
        {
            var team1 = new Team { Id = 8001, Name = "Alpha" };
            var team2 = new Team { Id = 8002, Name = "Beta" };

            await _data.Teams.AddRangeAsync(team1, team2);

            var match = new Match
            {
                Id = 9001,
                TournamentId = 4001,
                Team1Id = 8001,
                Team2Id = 8002,
                Team1Goals = 3,
                Team2Goals = 2
            };

            await _data.Matches.AddAsync(match);
            await _data.SaveChangesAsync();

            var result = await _tournamentService.GetEnterResultModelAsync(9001);

            Assert.NotNull(result);
            Assert.That(result!.MatchId, Is.EqualTo(9001));
            Assert.That(result.TournamentId, Is.EqualTo(4001));
            Assert.That(result.Team1Id, Is.EqualTo(8001));
            Assert.That(result.Team2Id, Is.EqualTo(8002));
            Assert.That(result.Team1Name, Is.EqualTo("Alpha"));
            Assert.That(result.Team2Name, Is.EqualTo("Beta"));
            Assert.That(result.Team1Goals, Is.EqualTo(3));
            Assert.That(result.Team2Goals, Is.EqualTo(2));
        }

        [Test]
        public async Task GetEnterResultModelAsync_ShouldHandleNullTeams()
        {
            var match = new Match
            {
                Id = 9002,
                TournamentId = 4002,
                Team1Id = null,
                Team2Id = null,
                Team1Goals = null,
                Team2Goals = null
            };

            await _data.Matches.AddAsync(match);
            await _data.SaveChangesAsync();

            var result = await _tournamentService.GetEnterResultModelAsync(9002);

            Assert.NotNull(result);
            Assert.That(result!.Team1Id, Is.Null);
            Assert.That(result.Team2Id, Is.Null);
            Assert.That(result.Team1Name, Is.Null);
            Assert.That(result.Team2Name, Is.Null);
            Assert.That(result.Team1Goals, Is.Null);
            Assert.That(result.Team2Goals, Is.Null);
        }

        [Test]
        public async Task EnterMatchResultAsync_ShouldReturnFalse_WhenMatchNotFound()
        {
            var model = new EnterResultViewModel
            {
                MatchId = 99999,
                Team1Goals = 1,
                Team2Goals = 0
            };

            var result = await _tournamentService.EnterMatchResultAsync(model);

            Assert.That(result.ok, Is.False);
            Assert.That(result.error, Is.EqualTo("Match not found."));
            Assert.That(result.tournamentId, Is.EqualTo(0));
        }

        [Test]
        public async Task EnterMatchResultAsync_ShouldReturnFalse_WhenGoalsNegative()
        {
            var match = new Match { Id = 9101, TournamentId = 5001, Team1Id = 1, Team2Id = 2 };
            await _data.Matches.AddAsync(match);
            await _data.SaveChangesAsync();

            var model = new EnterResultViewModel
            {
                MatchId = 9101,
                Team1Goals = -1,
                Team2Goals = 0
            };

            var result = await _tournamentService.EnterMatchResultAsync(model);

            Assert.That(result.ok, Is.False);
            Assert.That(result.error, Is.EqualTo("Головете не могат да са отрицателни."));
            Assert.That(result.tournamentId, Is.EqualTo(5001));

            var reloaded = await _data.Matches.FirstAsync(m => m.Id == 9101);
            Assert.IsNull(reloaded.Team1Goals);
            Assert.IsNull(reloaded.Team2Goals);
            Assert.IsNull(reloaded.WinnerTeamId);
        }

        [Test]
        public async Task EnterMatchResultAsync_ShouldReturnFalse_WhenDraw()
        {
            var match = new Match { Id = 9102, TournamentId = 5002, Team1Id = 1, Team2Id = 2 };
            await _data.Matches.AddAsync(match);
            await _data.SaveChangesAsync();

            var model = new EnterResultViewModel
            {
                MatchId = 9102,
                Team1Goals = 2,
                Team2Goals = 2
            };

            var result = await _tournamentService.EnterMatchResultAsync(model);

            Assert.That(result.ok, Is.False);
            Assert.That(result.error, Is.EqualTo("Равенство не е позволено. Моля, въведи победител."));
            Assert.That(result.tournamentId, Is.EqualTo(5002));

            var reloaded = await _data.Matches.FirstAsync(m => m.Id == 9102);
            Assert.IsNull(reloaded.Team1Goals);
            Assert.IsNull(reloaded.Team2Goals);
            Assert.IsNull(reloaded.WinnerTeamId);
        }

        [Test]
        public async Task EnterMatchResultAsync_ShouldSaveResult_AndSetWinnerTeamId_ToTeam1_WhenTeam1Wins()
        {
            var match = new Match
            {
                Id = 9103,
                TournamentId = 5003,
                Round = 1,
                IndexInRound = 0,
                Team1Id = 11,
                Team2Id = 22
            };

            var nextMatch = new Match
            {
                Id = 9104,
                TournamentId = 5003,
                Round = 2,
                IndexInRound = 0,
                Team1Id = null,
                Team2Id = null
            };

            await _data.Matches.AddRangeAsync(match, nextMatch);
            await _data.SaveChangesAsync();

            var model = new EnterResultViewModel
            {
                MatchId = 9103,
                Team1Goals = 3,
                Team2Goals = 1
            };

            var result = await _tournamentService.EnterMatchResultAsync(model);

            Assert.That(result.ok, Is.True);
            Assert.IsNull(result.error);
            Assert.That(result.tournamentId, Is.EqualTo(5003));

            var updated = await _data.Matches.FirstAsync(m => m.Id == 9103);
            Assert.That(updated.Team1Goals, Is.EqualTo(3));
            Assert.That(updated.Team2Goals, Is.EqualTo(1));
            Assert.That(updated.WinnerTeamId, Is.EqualTo(11));

            var updatedNext = await _data.Matches.FirstAsync(m => m.Id == 9104);
            Assert.That(updatedNext.Team1Id, Is.EqualTo(11));
        }

        [Test]
        public async Task EnterMatchResultAsync_ShouldSaveResult_AndSetWinnerTeamId_ToTeam2_WhenTeam2Wins_AndMoveToNextMatchTeam2_WhenIndexOdd()
        {
            var match = new Match
            {
                Id = 9201,
                TournamentId = 5004,
                Round = 1,
                IndexInRound = 1,
                Team1Id = 11,
                Team2Id = 22
            };

            var nextMatch = new Match
            {
                Id = 9202,
                TournamentId = 5004,
                Round = 2,
                IndexInRound = 0,
                Team1Id = null,
                Team2Id = null
            };

            await _data.Matches.AddRangeAsync(match, nextMatch);
            await _data.SaveChangesAsync();

            var model = new EnterResultViewModel
            {
                MatchId = 9201,
                Team1Goals = 0,
                Team2Goals = 2
            };

            var result = await _tournamentService.EnterMatchResultAsync(model);

            Assert.That(result.ok, Is.True);
            Assert.IsNull(result.error);
            Assert.That(result.tournamentId, Is.EqualTo(5004));

            var updated = await _data.Matches.FirstAsync(m => m.Id == 9201);
            Assert.That(updated.Team1Goals, Is.EqualTo(0));
            Assert.That(updated.Team2Goals, Is.EqualTo(2));
            Assert.That(updated.WinnerTeamId, Is.EqualTo(22));

            var updatedNext = await _data.Matches.FirstAsync(m => m.Id == 9202);
            Assert.That(updatedNext.Team2Id, Is.EqualTo(22));
        }


    }
}
