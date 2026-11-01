using FootballProjectSoftUni.Core.Models.Settings;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Encodings.Web;

namespace FootballProjectSoftUni.Services
{
    /// <summary>
    /// The "confirm your e-mail" link: after registering and from the "send the link again" page.
    /// At most one e-mail per account every few minutes, so nobody can flood a mailbox with it
    /// (or use up the daily limit of the site's mail account).
    /// </summary>
    public class ConfirmationEmailSender
    {
        public static readonly TimeSpan Cooldown = TimeSpan.FromMinutes(5);

        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender emailSender;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly IMemoryCache sentRecently;
        private readonly SiteSettings site;

        public ConfirmationEmailSender(
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            IStringLocalizer<SharedResource> localizer,
            IMemoryCache sentRecently,
            IOptions<SiteSettings> siteOptions)
        {
            this.userManager = userManager;
            this.emailSender = emailSender;
            this.localizer = localizer;
            this.sentRecently = sentRecently;
            site = siteOptions.Value;
        }

        public async Task SendAsync(ApplicationUser user, IUrlHelper url, HttpRequest request, string? returnUrl = null)
        {
            var key = "confirmation-email:" + user.Id;
            if (sentRecently.TryGetValue(key, out _))
            {
                return;
            }

            sentRecently.Set(key, true, Cooldown);

            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            // Absolute link from Site:BaseUrl - never from the request's Host header (host-header poisoning).
            var callbackUrl = site.Absolute(url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code, returnUrl })!, request);

            // In the language the visitor is using the site in; sent in the background.
            await emailSender.SendEmailAsync(user.Email!, localizer["EmailConfirmSubject"].Value,
                localizer["EmailConfirmBody", HtmlEncoder.Default.Encode(callbackUrl)].Value);
        }
    }
}
