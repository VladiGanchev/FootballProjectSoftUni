using FootballProjectSoftUni.Core.Constants;
using FootballProjectSoftUni.Core.Contracts.Message;
using FootballProjectSoftUni.Core.Models.ContactMessage;
using FootballProjectSoftUni.Core.Models.Message;
using FootballProjectSoftUni.Infrastructure.Data;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace FootballProjectSoftUni.Core.Services.Message
{
    public class ContactMessageService : IContactMessageService
    {
        private readonly ApplicationDbContext data;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;


        public ContactMessageService(ApplicationDbContext _data, UserManager<ApplicationUser> _userManager, RoleManager<IdentityRole> _roleManager)
        {
            data = _data;
            userManager = _userManager;
            roleManager = _roleManager;
        }

        public async Task<int> SendInitialAsync(string userId, string subject, string content, string? receiverUserId = null)
        {
            if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentException("Subject and content are required.");
            }

            // Ако няма подаден receiver → старият сценарий: потребител пише на админ
            if (receiverUserId == null)
            {
                var message = new ContactMessage
                {
                    UserId = userId,           // собственик на нишката е потребителят
                    Subject = subject,
                    Content = content,
                    IsFromAdmin = false,
                    ParentMessageId = null,
                    CreatedOn = DateTime.UtcNow
                };

                await data.ContactMessages.AddAsync(message);
                await data.SaveChangesAsync();

                var notification = new FootballProjectSoftUni.Infrastructure.Data.Models.Notification
                {
                    UserId = await GetAdminIdAsync(),   // получател е админът
                    ContactMessageId = message.Id,
                    Message = subject,
                    CreatedOn = DateTime.UtcNow,
                    IsRead = false
                };

                await data.Notifications.AddAsync(notification);
                await data.SaveChangesAsync();

                return message.Id;
            }
            else
            {
                // НОВО: админ пише директно на конкретен потребител (рефер)
                // userId тук е изпращачът (админ), receiverUserId – реферът
                bool isAdmin = await IsAdminAsync(userId);
                if (!isAdmin)
                {
                    throw new InvalidOperationException("Only admin can send initial message to specific user.");
                }

                var message = new ContactMessage
                {
                    UserId = receiverUserId,   // нишката "принадлежи" на рефера
                    Subject = subject,
                    Content = content,
                    IsFromAdmin = true,
                    ParentMessageId = null,
                    CreatedOn = DateTime.UtcNow
                };

                await data.ContactMessages.AddAsync(message);
                await data.SaveChangesAsync();

                var notification = new FootballProjectSoftUni.Infrastructure.Data.Models.Notification
                {
                    UserId = receiverUserId,   // нотификацията отива при рефера
                    ContactMessageId = message.Id,
                    Message = subject,
                    CreatedOn = DateTime.UtcNow,
                    IsRead = false
                };

                await data.Notifications.AddAsync(notification);
                await data.SaveChangesAsync();

                return message.Id;
            }
        }


        public async Task<int> ReplyAsync(int parentMessageId, string userId, string subject, string content)
        {
            var parent = await data.ContactMessages.FindAsync(parentMessageId);

            if (parent == null)
                throw new ArgumentException("Message not found.");

            bool fromAdmin = await IsAdminAsync(userId);
            string receiverId = fromAdmin ? parent.UserId : await GetAdminIdAsync();

            var subjectToUse = string.IsNullOrWhiteSpace(subject)
                ? parent.Subject
                : subject;

            var reply = new ContactMessage
            {
                UserId = parent.UserId,
                ParentMessageId = parent.Id,
                Subject = subjectToUse,
                Content = content,
                IsFromAdmin = fromAdmin,
                CreatedOn = DateTime.UtcNow
            };

            await data.ContactMessages.AddAsync(reply);
            await data.SaveChangesAsync();

            await data.Notifications.AddAsync(new FootballProjectSoftUni.Infrastructure.Data.Models.Notification
            {
                UserId = receiverId,
                ContactMessageId = reply.Id,
                Message = subjectToUse,
                IsRead = false,
                CreatedOn = DateTime.UtcNow
            });

            await data.SaveChangesAsync();
            return reply.Id;
        }

        public async Task<ReplyFormViewModel> GetReplyModelAsync(int messageId, string currentUserId)
        {
            var message = await data.ContactMessages
                .Include(m => m.ParentMessage)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
                throw new ArgumentException("Message not found.");

            var isAdmin = await IsAdminAsync(currentUserId);

            if (message.UserId != currentUserId && !isAdmin)
                throw new UnauthorizedAccessException("Нямате достъп до това съобщение.");

            bool isSender =
                (message.IsFromAdmin && isAdmin) ||
                (!message.IsFromAdmin && message.UserId == currentUserId);

            bool canReply = !isSender;

            var model = new ReplyFormViewModel
            {
                ParentMessageId = message.Id,
                Subject = message.Subject,
                CurrentMessageText = message.Content,
                PreviousMessageText = message.ParentMessage?.Content ?? string.Empty,
                CanReply = canReply
            };

            return model;
        }

        private async Task<bool> IsAdminAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
                return false;

            return await userManager.IsInRoleAsync(user, RoleConstants.AdminRole);
        }

        private async Task<string> GetAdminIdAsync()
        {
            var admins = await userManager.GetUsersInRoleAsync(RoleConstants.AdminRole);

            if (!admins.Any())
                throw new InvalidOperationException("❗ Не е намерен администратор. Увери се, че CreateAdminRoleAsync е извикан при стартиране.");

            return admins.First().Id;
        }


        public async Task<IPagedList<SentMessageViewModel>> GetSentMessagesAsync(string currentUserId, int pageNumber, int pageSize)
        {
            bool isAdmin = await IsAdminAsync(currentUserId);

            IQueryable<ContactMessage> query = data.ContactMessages;

            if (isAdmin)
            {
                query = query.Where(m => m.IsFromAdmin);
            }
            else
            {
                query = query.Where(m => m.UserId == currentUserId && !m.IsFromAdmin);
            }

            return await query
                .OrderByDescending(m => m.CreatedOn)
                .Select(m => new SentMessageViewModel
                {
                    Id = m.Id,
                    Subject = m.Subject,
                    Preview = m.Content.Length > 50 ? m.Content.Substring(0, 50) + "..." : m.Content,
                    CreatedOn = m.CreatedOn,
                    ToName = isAdmin
                        ? (m.User.FirstName + " " + m.User.LastName)
                        : "Админ"
                })
                .ToPagedListAsync(pageNumber, pageSize);
        }

    }
}
