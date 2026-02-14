using FootballProjectSoftUni.Core.Contracts.Email;
using FootballProjectSoftUni.Core.Contracts.Notification;
using FootballProjectSoftUni.Core.Services.TournamentReminder;
using FootballProjectSoftUni.Infrastructure.Data.Enums;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    public class TournamentReminderServiceTests : UnitTestsBase
    {
        private class FakeEmailService : IEmailService
        {
            public List<(string to, string subject, string body)> Sent { get; } = new();
            public Task SendAsync(string to, string subject, string htmlBody)
            {
                Sent.Add((to, subject, htmlBody));
                return Task.CompletedTask;
            }
        }

        private class FakeNotificationService : INotificationService
        {
            public List<(string userId, string message)> Created { get; } = new();

            public Task CreateNotificationForUserAsync(string userId, string message)
            {
                Created.Add((userId, message));
                return Task.CompletedTask;
            }

            public Task<IPagedList<FootballProjectSoftUni.Core.Models.Notification.NotificationViewModel>> AllNotificationsAsync(string userId, int pageNumber, int pageSize)
                => throw new NotImplementedException();
            public Task CreateNotificationForCityCoachesAsync(int cityId, string message)
                => throw new NotImplementedException();
            public Task<int> GetUnreadCountAsync(string userId)
                => throw new NotImplementedException();
            public Task MarkAllAsReadAsync(string userId)
                => throw new NotImplementedException();
            public Task DeleteAsync(int id, string userId)
                => throw new NotImplementedException();
        }

        [Test]
        public async Task Send24HourRemindersAsync_ShouldSendNotificationsAndEmails_AndMarkReminderSent()
        {
            var fakeEmail = new FakeEmailService();
            var fakeNotif = new FakeNotificationService();
            var service = new TournamentReminderService(_data, fakeNotif, fakeEmail);

            var start = DateTime.Now.AddHours(24);

            var tournament = new Tournament
            {
                Id = 8001,
                StartDate = start,
                EndDate = start.AddHours(2),
                CreatedOn = DateTime.Now,
                Description = "T",
                OrganiserId = "org",
                NumberOfTeams = 0,
                ImageUrl = "img",
                Status = TournamentStatus.Upcoming,
                Prize = 0,
                ParticipationFee = 0,
                ReminderSent = false
            };

            var city = new City { Id = 999, Name = "TestCity", ImageUrl = "img" };
            var user1 = new ApplicationUser { Id = "u1", Email = "u1@test.com", FirstName = "A", LastName = "B", UserName = "u1" };
            var user2 = new ApplicationUser { Id = "u2", Email = "u2@test.com", FirstName = "C", LastName = "D", UserName = "u2" };

            await _data.Cities.AddAsync(city);
            await _data.Users.AddRangeAsync(user1, user2);
            await _data.Tournaments.AddAsync(tournament);
            await _data.TournamentsCities.AddAsync(new TournamentCity { TournamentId = 8001, CityId = 999 });
            await _data.TournamentsParticipants.AddRangeAsync(
                new TournamentParticipant { ParticipantId = "u1", TournamentId = 8001, Role = "Player" },
                new TournamentParticipant { ParticipantId = "u2", TournamentId = 8001, Role = "Coach" }
            );
            await _data.SaveChangesAsync();

            await service.Send24HourRemindersAsync();

            Assert.That(fakeNotif.Created.Count, Is.EqualTo(2));
            Assert.That(fakeNotif.Created.Any(x => x.userId == "u1"), Is.True);
            Assert.That(fakeNotif.Created.Any(x => x.userId == "u2"), Is.True);

            Assert.That(fakeEmail.Sent.Count, Is.EqualTo(2));
            Assert.That(fakeEmail.Sent.Any(x => x.to == "u1@test.com" && x.subject == "Напомняне за турнир"), Is.True);
            Assert.That(fakeEmail.Sent.Any(x => x.to == "u2@test.com" && x.subject == "Напомняне за турнир"), Is.True);

            var updated = await _data.Tournaments.FirstAsync(t => t.Id == 8001);
            Assert.That(updated.ReminderSent, Is.True);
        }

        [Test]
        public async Task Send24HourRemindersAsync_ShouldNotSend_WhenTournamentOutsideWindow()
        {
            var fakeEmail = new FakeEmailService();
            var fakeNotif = new FakeNotificationService();
            var service = new TournamentReminderService(_data, fakeNotif, fakeEmail);

            var start = DateTime.Now.AddHours(26);

            var tournament = new Tournament
            {
                Id = 8002,
                StartDate = start,
                EndDate = start.AddHours(2),
                CreatedOn = DateTime.Now,
                Description = "T",
                OrganiserId = "org",
                NumberOfTeams = 0,
                ImageUrl = "img",
                Status = TournamentStatus.Upcoming,
                Prize = 0,
                ParticipationFee = 0,
                ReminderSent = false
            };

            await _data.Tournaments.AddAsync(tournament);
            await _data.SaveChangesAsync();

            await service.Send24HourRemindersAsync();

            Assert.That(fakeNotif.Created.Count, Is.EqualTo(0));
            Assert.That(fakeEmail.Sent.Count, Is.EqualTo(0));

            var updated = await _data.Tournaments.FirstAsync(t => t.Id == 8002);
            Assert.That(updated.ReminderSent, Is.False);
        }

        [Test]
        public async Task Send24HourRemindersAsync_ShouldNotSendTwice_WhenReminderAlreadySent()
        {
            var fakeEmail = new FakeEmailService();
            var fakeNotif = new FakeNotificationService();
            var service = new TournamentReminderService(_data, fakeNotif, fakeEmail);

            var start = DateTime.Now.AddHours(24);

            var tournament = new Tournament
            {
                Id = 8003,
                StartDate = start,
                EndDate = start.AddHours(2),
                CreatedOn = DateTime.Now,
                Description = "T",
                OrganiserId = "org",
                NumberOfTeams = 0,
                ImageUrl = "img",
                Status = TournamentStatus.Upcoming,
                Prize = 0,
                ParticipationFee = 0,
                ReminderSent = true
            };

            await _data.Tournaments.AddAsync(tournament);
            await _data.SaveChangesAsync();

            await service.Send24HourRemindersAsync();

            Assert.That(fakeNotif.Created.Count, Is.EqualTo(0));
            Assert.That(fakeEmail.Sent.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task Send24HourRemindersAsync_ShouldMarkReminderSent_WhenNoParticipants()
        {
            var fakeEmail = new FakeEmailService();
            var fakeNotif = new FakeNotificationService();
            var service = new TournamentReminderService(_data, fakeNotif, fakeEmail);

            var start = DateTime.Now.AddHours(24);

            var tournament = new Tournament
            {
                Id = 8004,
                StartDate = start,
                EndDate = start.AddHours(2),
                CreatedOn = DateTime.Now,
                Description = "T",
                OrganiserId = "org",
                NumberOfTeams = 0,
                ImageUrl = "img",
                Status = TournamentStatus.Upcoming,
                Prize = 0,
                ParticipationFee = 0,
                ReminderSent = false
            };

            await _data.Tournaments.AddAsync(tournament);
            await _data.SaveChangesAsync();

            await service.Send24HourRemindersAsync();

            Assert.That(fakeNotif.Created.Count, Is.EqualTo(0));
            Assert.That(fakeEmail.Sent.Count, Is.EqualTo(0));

            var updated = await _data.Tournaments.FirstAsync(t => t.Id == 8004);
            Assert.That(updated.ReminderSent, Is.True);
        }

        [Test]
        public async Task Send24HourRemindersAsync_ShouldSendNotificationButSkipEmail_WhenUserEmailIsNullOrWhitespace()
        {
            var fakeEmail = new FakeEmailService();
            var fakeNotif = new FakeNotificationService();
            var service = new TournamentReminderService(_data, fakeNotif, fakeEmail);

            var start = DateTime.Now.AddHours(24);

            var tournament = new Tournament
            {
                Id = 8005,
                StartDate = start,
                EndDate = start.AddHours(2),
                CreatedOn = DateTime.Now,
                Description = "T",
                OrganiserId = "org",
                NumberOfTeams = 0,
                ImageUrl = "img",
                Status = TournamentStatus.Upcoming,
                Prize = 0,
                ParticipationFee = 0,
                ReminderSent = false
            };

            var user = new ApplicationUser { Id = "u3", Email = " ", FirstName = "E", LastName = "F", UserName = "u3" };

            await _data.Users.AddAsync(user);
            await _data.Tournaments.AddAsync(tournament);
            await _data.TournamentsParticipants.AddAsync(new TournamentParticipant { ParticipantId = "u3", TournamentId = 8005, Role = "Player" });
            await _data.SaveChangesAsync();

            await service.Send24HourRemindersAsync();

            Assert.That(fakeNotif.Created.Count, Is.EqualTo(1));
            Assert.That(fakeNotif.Created[0].userId, Is.EqualTo("u3"));
            Assert.That(fakeEmail.Sent.Count, Is.EqualTo(0));

            var updated = await _data.Tournaments.FirstAsync(t => t.Id == 8005);
            Assert.That(updated.ReminderSent, Is.True);
        }
    }
}
