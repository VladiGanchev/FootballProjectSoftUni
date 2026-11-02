namespace FootballProjectSoftUni.Core.Extensions
{
    /// <summary>
    /// How long browsers may keep the site's own files (CSS, scripts, fonts, pictures) without asking again.
    /// A file that can never change at its address is kept for a year; any other file is checked on every visit
    /// (the server answers "not changed" with a few bytes), so a new version is never missed after a deployment.
    /// </summary>
    public static class StaticFileCache
    {
        public const string Year = "public, max-age=31536000, immutable";
        public const string Month = "public, max-age=2592000";
        public const string AskEveryTime = "no-cache";

        /// <param name="path">The file's address, for example "/css/site.css".</param>
        /// <param name="hasQuery">The address carries a version, such as "?v=..." from asp-append-version.</param>
        public static string For(string path, bool hasQuery)
        {
            // asp-append-version (and the icon font's own "?hash") put the file's content in its address:
            // a changed file gets a new address.
            if (hasQuery)
            {
                return Year;
            }

            // Uploaded pictures get a new random name every time and are never overwritten.
            if (path.StartsWith("/images/", StringComparison.OrdinalIgnoreCase))
            {
                return Year;
            }

            // Fonts practically never change.
            if (path.EndsWith(".woff2", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".woff", StringComparison.OrdinalIgnoreCase))
            {
                return Month;
            }

            return AskEveryTime;
        }
    }
}
