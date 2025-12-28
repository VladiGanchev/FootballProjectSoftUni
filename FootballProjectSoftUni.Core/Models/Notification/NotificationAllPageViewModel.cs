using FootballProjectSoftUni.Core.Models.ContactMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace FootballProjectSoftUni.Core.Models.Notification
{
    public class NotificationAllPageViewModel
    {
        public IPagedList<NotificationViewModel> Notifications { get; set; }
            = new PagedList<NotificationViewModel>(new List<NotificationViewModel>(), 1, 1);

        public IPagedList<SentMessageViewModel> SentMessages { get; set; }
            = new PagedList<SentMessageViewModel>(new List<SentMessageViewModel>(), 1, 1);

        public string Box { get; set; } = "inbox";
    }
}
