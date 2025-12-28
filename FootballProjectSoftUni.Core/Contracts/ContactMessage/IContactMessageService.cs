using FootballProjectSoftUni.Core.Models.ContactMessage;
using FootballProjectSoftUni.Core.Models.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace FootballProjectSoftUni.Core.Contracts.Message
{
    public interface IContactMessageService
    {
        Task<int> SendInitialAsync(string userId, string subject, string content);
        Task<int> ReplyAsync(int parentMessageId, string userId, string subject, string content);
        Task<ReplyFormViewModel> GetReplyModelAsync(int messageId, string currentUserId);

        Task<IPagedList<SentMessageViewModel>> GetSentMessagesAsync(string currentUserId, int pageNumber, int pageSize);
    }

}
