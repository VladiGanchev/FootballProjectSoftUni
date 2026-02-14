using FootballProjectSoftUni.Core.Contracts.Email;
using FootballProjectSoftUni.Core.Contracts.Notification;
using FootballProjectSoftUni.Core.Contracts.Partner;
using FootballProjectSoftUni.Core.Models.Email;
using FootballProjectSoftUni.Core.Services.Email;
using FootballProjectSoftUni.Core.Services.Notification;
using FootballProjectSoftUni.Core.Services.Partner;
using FootballProjectSoftUni.Infrastructure.Data.Enums;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    public class FakeEmailService : IEmailService
    {
        public List<(string to, string subject, string content)> Sent { get; } = new();

        public Task SendAsync(string to, string subject, string content)
        {
            Sent.Add((to, subject, content));
            return Task.CompletedTask;
        }
    }

    public class NotificationServiceTests : UnitTestsBase
    {
        private INotificationService notificationService;
        private IEmailService emailService;

        [SetUp]
        public void SetUp()
        {
            var emailOptions = Options.Create(new EmailSettings
            {
                From = "Test <test@test.com>",
                SmtpHost = "localhost",
                SmtpPort = 25,
                EnableSsl = false,
                SmtpUser = "user",
                SmtpPass = "pass",
                OverrideTo = "override@test.com"
            });

            emailService = new EmailService(emailOptions);
            notificationService = new NotificationService(_data, emailService);
        }

        [Test]
        public async Task GetUnreadCountAsync_ShouldReturn0_WhenNoNotifications()
        {
            var count = await notificationService.GetUnreadCountAsync("u1");
            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetUnreadCountAsync_ShouldCountOnlyUnread_ForUser()
        {
            await _data.Notifications.AddRangeAsync(
                new FootballProjectSoftUni.Infrastructure.Data.Models.Notification
                {
                    UserId = "u1",
                    Message = "A",
                    CreatedOn = DateTime.UtcNow,
                    IsRead = false
                },
                new FootballProjectSoftUni.Infrastructure.Data.Models.Notification
                {
                    UserId = "u1",
                    Message = "B",
                    CreatedOn = DateTime.UtcNow,
                    IsRead = true
                },
                new FootballProjectSoftUni.Infrastructure.Data.Models.Notification
                {
                    UserId = "u2",
                    Message = "C",
                    CreatedOn = DateTime.UtcNow,
                    IsRead = false
                }
            );
            await _data.SaveChangesAsync();

            var count = await notificationService.GetUnreadCountAsync("u1");
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public async Task MarkAllAsReadAsync_ShouldMarkOnlyUnread_ForUser()
        {
            await _data.Notifications.AddRangeAsync(
                new FootballProjectSoftUni.Infrastructure.Data.Models.Notification
                {
                    UserId = "u1",
                    Message = "A",
                    CreatedOn = DateTime.UtcNow,
                    IsRead = false
                },
                new FootballProjectSoftUni.Infrastructure.Data.Models.Notification
                {
                    UserId = "u1",
                    Message = "B",
                    CreatedOn = DateTime.UtcNow,
                    IsRead = false
                },
                new FootballProjectSoftUni.Infrastructure.Data.Models.Notification
                {
                    UserId = "u2",
                    Message = "C",
                    CreatedOn = DateTime.UtcNow,
                    IsRead = false
                }
            );
            await _data.SaveChangesAsync();

            await notificationService.MarkAllAsReadAsync("u1");

            var u1Unread = await _data.Notifications.CountAsync(n => n.UserId == "u1" && !n.IsRead);
            var u2Unread = await _data.Notifications.CountAsync(n => n.UserId == "u2" && !n.IsRead);

            Assert.That(u1Unread, Is.EqualTo(0));
            Assert.That(u2Unread, Is.EqualTo(1));
        }

        [Test]
        public async Task DeleteAsync_ShouldRemoveNotification_WhenMatchesIdAndUser()
        {
            var n = new FootballProjectSoftUni.Infrastructure.Data.Models.Notification
            {
                UserId = "u1",
                Message = "Del",
                CreatedOn = DateTime.UtcNow,
                IsRead = false
            };

            await _data.Notifications.AddAsync(n);
            await _data.SaveChangesAsync();

            await notificationService.DeleteAsync(n.Id, "u1");

            var exists = await _data.Notifications.AnyAsync(x => x.Id == n.Id);
            Assert.That(exists, Is.False);
        }

        [Test]
        public async Task DeleteAsync_ShouldDoNothing_WhenWrongUser()
        {
            var n = new FootballProjectSoftUni.Infrastructure.Data.Models.Notification
            {
                UserId = "u1",
                Message = "Del",
                CreatedOn = DateTime.UtcNow,
                IsRead = false
            };

            await _data.Notifications.AddAsync(n);
            await _data.SaveChangesAsync();

            await notificationService.DeleteAsync(n.Id, "u2");

            var exists = await _data.Notifications.AnyAsync(x => x.Id == n.Id);
            Assert.That(exists, Is.True);
        }

        [Test]
        public async Task CreateNotificationForUserAsync_ShouldCreateUnreadNotification()
        {
            await notificationService.CreateNotificationForUserAsync("u1", "Hello");

            var created = await _data.Notifications
                .OrderByDescending(n => n.Id)
                .FirstAsync();

            Assert.That(created.UserId, Is.EqualTo("u1"));
            Assert.That(created.Message, Is.EqualTo("Hello"));
            Assert.That(created.IsRead, Is.False);
            Assert.That(created.ContactMessageId, Is.Null);
        }

        [Test]
        public async Task AllNotificationsAsync_ShouldReturnOnlyUserNotifications_OrderedByCreatedOnDesc()
        {
            await _data.Notifications.AddRangeAsync(
                new FootballProjectSoftUni.Infrastructure.Data.Models.Notification
                {
                    UserId = "u1",
                    Message = "Old",
                    CreatedOn = new DateTime(2020, 1, 1),
                    IsRead = false
                },
                new FootballProjectSoftUni.Infrastructure.Data.Models.Notification
                {
                    UserId = "u1",
                    Message = "New",
                    CreatedOn = new DateTime(2021, 1, 1),
                    IsRead = true
                },
                new FootballProjectSoftUni.Infrastructure.Data.Models.Notification
                {
                    UserId = "u2",
                    Message = "Other",
                    CreatedOn = new DateTime(2022, 1, 1),
                    IsRead = false
                }
            );
            await _data.SaveChangesAsync();

            var result = await notificationService.AllNotificationsAsync("u1", pageNumber: 1, pageSize: 10);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Message, Is.EqualTo("New"));
            Assert.That(result[1].Message, Is.EqualTo("Old"));
        }

        [Test]
        public async Task CreateNotificationForCityCoachesAsync_ShouldReturn_WhenNoCoachParticipantsInCity()
        {
            var fakeEmail = new FakeEmailService();
            var service = new NotificationService(_data, fakeEmail);

            await service.CreateNotificationForCityCoachesAsync(cityId: 1, message: "Msg");

            var anyNotifications = await _data.Notifications.AnyAsync();
            Assert.That(anyNotifications, Is.False);
            Assert.That(fakeEmail.Sent.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task CreateNotificationForCityCoachesAsync_ShouldCreateNotifications_AndSendEmails_ToDistinctCoachIds()
        {
            var fakeEmail = new FakeEmailService();
            var service = new NotificationService(_data, fakeEmail);

            var coachUser1 = new ApplicationUser { Id = "coach1", Email = "c1@test.com", FirstName = "C1", LastName = "L1", UserName = "c1" };
            var coachUser2 = new ApplicationUser { Id = "coach2", Email = "c2@test.com", FirstName = "C2", LastName = "L2", UserName = "c2" };
            await _data.Users.AddRangeAsync(coachUser1, coachUser2);

            var t1 = new Tournament
            {
                Id = 7001,
                StartDate = DateTime.Now.AddDays(5),
                EndDate = DateTime.Now.AddDays(10),
                Description = "T1",
                OrganiserId = "org1",
                NumberOfTeams = 0,
                CreatedOn = DateTime.Now,
                ImageUrl = "img",
                Status = TournamentStatus.Upcoming,
                Prize = 0,
                ParticipationFee = 0
            };

            var t2 = new Tournament
            {
                Id = 7002,
                StartDate = DateTime.Now.AddDays(6),
                EndDate = DateTime.Now.AddDays(11),
                Description = "T2",
                OrganiserId = "org1",
                NumberOfTeams = 0,
                CreatedOn = DateTime.Now,
                ImageUrl = "img",
                Status = TournamentStatus.Upcoming,
                Prize = 0,
                ParticipationFee = 0
            };

            await _data.Tournaments.AddRangeAsync(t1, t2);

            await _data.TournamentsCities.AddRangeAsync(
                new TournamentCity { TournamentId = 7001, CityId = 1 },
                new TournamentCity { TournamentId = 7002, CityId = 1 }
            );

            await _data.TournamentsParticipants.AddRangeAsync(
                new TournamentParticipant { ParticipantId = "coach1", TournamentId = 7001, Role = "Coach" },
                new TournamentParticipant { ParticipantId = "coach2", TournamentId = 7001, Role = "Coach" },
                new TournamentParticipant { ParticipantId = "coach2", TournamentId = 7002, Role = "Coach" }
            );

            await _data.SaveChangesAsync();

            await service.CreateNotificationForCityCoachesAsync(cityId: 1, message: "Hello Coaches");

            var notifications = await _data.Notifications.Where(n => n.Message == "Hello Coaches").ToListAsync();
            Assert.That(notifications.Count, Is.EqualTo(2));
            Assert.That(notifications.Any(n => n.UserId == "coach1"), Is.True);
            Assert.That(notifications.Any(n => n.UserId == "coach2"), Is.True);
            Assert.That(notifications.All(n => n.IsRead == false), Is.True);

            Assert.That(fakeEmail.Sent.Count, Is.EqualTo(2));
            Assert.That(fakeEmail.Sent.Any(x => x.to == "c1@test.com" && x.subject == "Нов Турнир!" && x.content == "Hello Coaches"), Is.True);
            Assert.That(fakeEmail.Sent.Any(x => x.to == "c2@test.com" && x.subject == "Нов Турнир!" && x.content == "Hello Coaches"), Is.True);
        }


    }
}
