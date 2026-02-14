using FootballProjectSoftUni.Core.Constants;
using FootballProjectSoftUni.Core.Services.Message;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    public class ContactMessageServiceTests : UnitTestsBase
    {
        private ContactMessageService service = null!;
        private UserManager<ApplicationUser> userManager = null!;
        private RoleManager<IdentityRole> roleManager = null!;

        [SetUp]
        public async Task SetUp()
        {
            var userStore = new UserStore<ApplicationUser>(_data);
            var roleStore = new RoleStore<IdentityRole>(_data);

            userManager = new UserManager<ApplicationUser>(
                userStore, null!, null!, null!, null!, null!, null!, null!, null!);

            roleManager = new RoleManager<IdentityRole>(
                roleStore, null!, null!, null!, null!);

            if (!await roleManager.RoleExistsAsync(RoleConstants.AdminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(RoleConstants.AdminRole));
            }

            service = new ContactMessageService(_data, userManager, roleManager);
        }

        [Test]
        public async Task SendInitialAsync_UserToAdmin_ShouldCreateMessageAndNotification()
        {
            var user = new ApplicationUser
            {
                Id = "u1",
                UserName = "u1@test.com",
                Email = "u1@test.com",
                FirstName = "User",
                LastName = "One"
            };

            var admin = new ApplicationUser
            {
                Id = "admin1",
                UserName = "admin@test.com",
                Email = "admin@test.com",
                FirstName = "Admin",
                LastName = "User"
            };

            await userManager.CreateAsync(user);
            await userManager.CreateAsync(admin);
            await userManager.AddToRoleAsync(admin, RoleConstants.AdminRole);

            var messageId = await service.SendInitialAsync(user.Id, "Subject", "Content");

            var msg = await _data.ContactMessages.FindAsync(messageId);
            Assert.NotNull(msg);
            Assert.That(msg!.UserId, Is.EqualTo(user.Id));
            Assert.That(msg.IsFromAdmin, Is.False);

            var notif = await _data.Notifications
                .FirstOrDefaultAsync(n => n.ContactMessageId == messageId);

            Assert.NotNull(notif);
            Assert.That(notif!.UserId, Is.EqualTo(admin.Id));
        }

        [Test]
        public async Task SendInitialAsync_AdminToUser_ShouldCreateAdminMessage()
        {
            var admin = new ApplicationUser
            {
                Id = "admin1",
                UserName = "admin@test.com",
                Email = "admin@test.com",
                FirstName = "Admin",
                LastName = "User"
            };

            var user = new ApplicationUser
            {
                Id = "u1",
                UserName = "u1@test.com",
                Email = "u1@test.com",
                FirstName = "User",
                LastName = "One"
            };

            await userManager.CreateAsync(admin);
            await userManager.CreateAsync(user);
            await userManager.AddToRoleAsync(admin, RoleConstants.AdminRole);

            var messageId = await service.SendInitialAsync(admin.Id, "Hello", "Msg", user.Id);

            var msg = await _data.ContactMessages.FindAsync(messageId);

            Assert.NotNull(msg);
            Assert.That(msg!.IsFromAdmin, Is.True);
            Assert.That(msg.UserId, Is.EqualTo(user.Id));
        }

        [Test]
        public async Task ReplyAsync_UserReply_ShouldNotifyAdmin()
        {
            var user = new ApplicationUser
            {
                Id = "u1",
                UserName = "u1@test.com",
                Email = "u1@test.com",
                FirstName = "User",
                LastName = "One"
            };

            var admin = new ApplicationUser
            {
                Id = "admin1",
                UserName = "admin@test.com",
                Email = "admin@test.com",
                FirstName = "Admin",
                LastName = "User"
            };

            await userManager.CreateAsync(user);
            await userManager.CreateAsync(admin);
            await userManager.AddToRoleAsync(admin, RoleConstants.AdminRole);

            var parentId = await service.SendInitialAsync(user.Id, "Subj", "Body");

            var replyId = await service.ReplyAsync(parentId, user.Id, "", "Reply");

            var reply = await _data.ContactMessages.FindAsync(replyId);
            Assert.NotNull(reply);
            Assert.That(reply!.ParentMessageId, Is.EqualTo(parentId));
            Assert.That(reply.IsFromAdmin, Is.False);

            var notif = await _data.Notifications
                .FirstOrDefaultAsync(n => n.ContactMessageId == replyId);

            Assert.NotNull(notif);
            Assert.That(notif!.UserId, Is.EqualTo(admin.Id));
        }
    }
}
