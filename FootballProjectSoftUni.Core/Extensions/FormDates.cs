using System.Globalization;

namespace FootballProjectSoftUni.Core.Extensions
{
    /// <summary>
    /// Dates in forms. The browser's date pickers always send ISO values ("2026-11-03T18:00", "1990-04-15"),
    /// whatever the language of the site, so nothing here depends on the current culture.
    /// The older text formats ("03/11/2026 18:00", "03.11.2026 18:00") are still accepted.
    /// </summary>
    public static class FormDates
    {
        public const string DateTimeInputFormat = "yyyy-MM-ddTHH:mm";
        public const string DateInputFormat = "yyyy-MM-dd";

        private static readonly string[] DateTimeFormats =
        {
            "yyyy-MM-ddTHH:mm",
            "yyyy-MM-ddTHH:mm:ss",
            "d/M/yyyy H:mm",
            "d.M.yyyy H:mm"
        };

        private static readonly string[] DateFormats =
        {
            "yyyy-MM-dd",
            "d.M.yyyy",
            "d/M/yyyy"
        };

        /// <summary>Value for an &lt;input type="datetime-local"&gt;.</summary>
        public static string ToDateTimeInput(this DateTime value) =>
            value.ToString(DateTimeInputFormat, CultureInfo.InvariantCulture);

        /// <summary>Value for an &lt;input type="date"&gt;.</summary>
        public static string ToDateInput(this DateTime value) =>
            value.ToString(DateInputFormat, CultureInfo.InvariantCulture);

        public static bool TryParseDateTime(string? value, out DateTime result) =>
            DateTime.TryParseExact((value ?? string.Empty).Trim(), DateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);

        public static bool TryParseDate(string? value, out DateTime result)
        {
            var text = (value ?? string.Empty).Trim();

            if (DateTime.TryParseExact(text, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return true;
            }

            // A date with a time (the old form asked for one) - only the day matters.
            if (TryParseDateTime(text, out result))
            {
                result = result.Date;
                return true;
            }

            return false;
        }
    }
}
