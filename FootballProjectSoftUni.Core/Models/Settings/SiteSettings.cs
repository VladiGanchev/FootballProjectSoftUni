using Microsoft.AspNetCore.Http;

namespace FootballProjectSoftUni.Core.Models.Settings
{
    /// <summary>
    /// Public address of the site ("Site" section in appsettings).
    /// Links that leave the site (password-reset / confirmation emails, Stripe return URLs)
    /// are built from this value and NOT from the request's Host header, which an attacker controls.
    /// </summary>
    public class SiteSettings
    {
        /// <summary>For example "https://tournamentgalaxy.bg". Empty in local development.</summary>
        public string BaseUrl { get; set; } = string.Empty;

        // Who runs the site, for the footer, the Terms and the Privacy Policy (the law wants them always visible).
        // Whatever is empty is simply not shown; fill them in once the company is registered.

        /// <summary>Person or company that runs the site.</summary>
        public string OperatorName { get; set; } = string.Empty;

        /// <summary>Company number (ЕИК).</summary>
        public string OperatorCompanyId { get; set; } = string.Empty;

        public string OperatorAddress { get; set; } = string.Empty;

        /// <summary>Public e-mail for questions, complaints and personal-data requests (also for visitors without an account).</summary>
        public string ContactEmail { get; set; } = string.Empty;

        public string ContactPhone { get; set; } = string.Empty;

        /// <summary>
        /// Absolute URL for <paramref name="relativeUrl"/>. Uses <see cref="BaseUrl"/> when it is configured;
        /// only falls back to the current request (local development) when it is not.
        /// </summary>
        public string Absolute(string relativeUrl, HttpRequest? request)
        {
            var path = relativeUrl.StartsWith("/") ? relativeUrl : "/" + relativeUrl;

            if (!string.IsNullOrWhiteSpace(BaseUrl))
            {
                return BaseUrl.TrimEnd('/') + path;
            }

            if (request == null)
            {
                throw new InvalidOperationException("Site:BaseUrl is not configured.");
            }

            return $"{request.Scheme}://{request.Host}{path}";
        }
    }
}
