namespace FootballProjectSoftUni.Core.Extensions
{
    /// <summary>
    /// The clock of the site: Bulgarian time (summer time included), whatever the time zone of the server.
    /// Tournament dates are typed in and stored in Bulgarian time, so "now" has to be Bulgarian time too
    /// (the hosting server runs an hour behind).
    /// </summary>
    public static class BgTime
    {
        private static readonly TimeZoneInfo Zone = FindZone();

        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zone);

        public static DateTime Today => Now.Date;

        /// <summary>A moment saved in UTC (payments, messages, notifications, comments) in Bulgarian time.</summary>
        public static DateTime FromUtc(DateTime utc) =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Zone);

        private static TimeZoneInfo FindZone()
        {
            // Windows and Linux name the zone differently.
            foreach (var id in new[] { "FLE Standard Time", "Europe/Sofia" })
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(id);
                }
                catch (TimeZoneNotFoundException)
                {
                }
                catch (InvalidTimeZoneException)
                {
                }
            }

            return TimeZoneInfo.Local;
        }
    }
}
