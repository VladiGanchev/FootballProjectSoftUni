using FootballProjectSoftUni.Core.Contracts.City;
using FootballProjectSoftUni.Core.Contracts.Coach;
using FootballProjectSoftUni.Core.Contracts.Referee;
using FootballProjectSoftUni.Core.Models.Referee;
using FootballProjectSoftUni.Core.Models.Tournament;
using FootballProjectSoftUni.Core.Services.City;
using FootballProjectSoftUni.Core.Services.Referee;
using FootballProjectSoftUni.Infrastructure.Data.Models;
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

        [OneTimeSetUp]

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

    }
}
