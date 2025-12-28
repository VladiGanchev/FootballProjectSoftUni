using FootballProjectSoftUni.Core.Contracts.Message;
using FootballProjectSoftUni.Core.Contracts.Notification;
using FootballProjectSoftUni.Core.Models.Message;
using FootballProjectSoftUni.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballProjectSoftUni.Controllers
{
    [Authorize]
    public class ContactMessageController : Controller
    {
        private readonly IContactMessageService messageService;

        public ContactMessageController(IContactMessageService _messageService)
        {
            messageService = _messageService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContactFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await messageService.SendInitialAsync(User.Id(), model.Subject, model.Content);
            return RedirectToAction("All", "Notification");
        }

        [HttpGet]
        public async Task<IActionResult> Reply(int messageId)
        {
            var model = await messageService.GetReplyModelAsync(messageId, User.Id());
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(ReplyFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await messageService.ReplyAsync(
                model.ParentMessageId,
                User.Id(),
                model.Subject,
                model.Content);

            return RedirectToAction("All", "Notification");
        }

        [HttpGet]
        public async Task<IActionResult> OpenMessage(int messageId)
        {
            var model = await messageService.GetReplyModelAsync(messageId, User.Id());
            return View("MessageWindow", model);
        }
    }
}
