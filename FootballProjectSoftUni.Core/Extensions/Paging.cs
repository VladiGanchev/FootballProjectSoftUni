namespace FootballProjectSoftUni.Core.Extensions
{
    /// <summary>
    /// The page of a list asked for with "?page=". X.PagedList throws for 0 or a negative page, and a huge
    /// page overflows the number of rows to skip; both used to end with an error page.
    /// </summary>
    public static class Paging
    {
        // Far beyond any real list, yet (page - 1) * page size can't overflow.
        public const int MaxPage = 100_000;

        public static int ValidPage(int? page) => Math.Clamp(page ?? 1, 1, MaxPage);
    }
}
