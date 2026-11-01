using FootballProjectSoftUni.Core.Models.Settings;
using Microsoft.AspNetCore.Http;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    /// <summary>
    /// Links in emails and Stripe return URLs must not depend on the request's Host header
    /// (host-header poisoning of password-reset links).
    /// </summary>
    public class SiteSettingsTests
    {
        private static HttpRequest RequestWithHost(string host)
        {
            var context = new DefaultHttpContext();
            context.Request.Scheme = "https";
            context.Request.Host = new HostString(host);
            return context.Request;
        }

        [Test]
        public void Absolute_WithBaseUrl_IgnoresAttackerControlledHost()
        {
            var site = new SiteSettings { BaseUrl = "https://tournamentgalaxy.bg" };

            var url = site.Absolute("/Identity/Account/ResetPassword?code=abc", RequestWithHost("evil.example"));

            Assert.That(url, Is.EqualTo("https://tournamentgalaxy.bg/Identity/Account/ResetPassword?code=abc"));
        }

        [Test]
        public void Absolute_TrimsTrailingSlashAndAddsLeadingSlash()
        {
            var site = new SiteSettings { BaseUrl = "https://tournamentgalaxy.bg/" };

            Assert.That(site.Absolute("City/PaymentSuccess?orderId=5", null),
                Is.EqualTo("https://tournamentgalaxy.bg/City/PaymentSuccess?orderId=5"));
        }

        [Test]
        public void Absolute_WithoutBaseUrl_UsesRequest_ForLocalDevelopment()
        {
            var site = new SiteSettings();

            Assert.That(site.Absolute("/x", RequestWithHost("localhost:7056")), Is.EqualTo("https://localhost:7056/x"));
        }

        [Test]
        public void Absolute_WithoutBaseUrlAndRequest_Throws()
        {
            var site = new SiteSettings();

            Assert.Throws<InvalidOperationException>(() => site.Absolute("/x", null));
        }
    }
}
