using Hangfire.Dashboard;

namespace FootballProjectSoftUni.Extensions
{
    /// <summary>
    /// Only signed-in administrators may open the Hangfire dashboard (/hangfire).
    /// Replaces Hangfire's default "local requests only" rule, which is not reliable
    /// behind a hosting provider's proxy.
    /// </summary>
    public class HangfireAdminAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var user = context.GetHttpContext().User;

            return user.Identity?.IsAuthenticated == true && user.IsAdmin();
        }
    }
}
