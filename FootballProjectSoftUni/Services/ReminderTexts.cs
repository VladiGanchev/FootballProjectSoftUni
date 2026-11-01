using FootballProjectSoftUni.Core.Contracts.TournamentReminder;
using FootballProjectSoftUni.Extensions;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Net;

namespace FootballProjectSoftUni.Services
{
    public class ReminderTexts : IReminderTexts
    {
        private readonly IStringLocalizer<SharedResource> localizer;

        public ReminderTexts(IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
        }

        public ReminderText For(string culture, string? city, DateTime start) =>
            CultureScope.Run(culture, () =>
            {
                var cityName = string.IsNullOrWhiteSpace(city) ? localizer["ReminderUnknownCity"].Value : city;
                var date = start.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
                var time = start.ToString("HH:mm", CultureInfo.InvariantCulture);

                return new ReminderText(
                    localizer["ReminderSubject"].Value,
                    localizer["ReminderNotification", date, time, cityName].Value,
                    localizer["ReminderEmailBody", date, time, WebUtility.HtmlEncode(cityName)].Value);
            });
    }
}
