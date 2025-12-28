using FootballProjectSoftUni.Core.Models.ContactMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Models.Notification
{
    public class NotificationAllPageViewModel
    {
        public IEnumerable<NotificationViewModel> Notifications { get; set; }
            = new List<NotificationViewModel>();

        public IEnumerable<SentMessageViewModel> SentMessages { get; set; }
            = new List<SentMessageViewModel>();

        public string Box { get; set; } = "inbox";
    }
}
