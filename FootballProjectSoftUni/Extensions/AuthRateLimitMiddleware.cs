using System.Collections.Concurrent;

namespace FootballProjectSoftUni.Extensions
{
    /// <summary>
    /// Limits how often one IP address can submit the login, register, password and account-deletion forms.
    /// Stops automated checking of which emails are registered and password spraying across
    /// many accounts (per-account lockout alone does not stop that). Normal users never hit it.
    /// </summary>
    public class AuthRateLimitMiddleware
    {
        public const int MaxAttemptsPerWindow = 10;
        public static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

        // Identity pages (area "Identity") whose forms check passwords or send e-mails.
        private static readonly HashSet<string> ProtectedPages = new(StringComparer.OrdinalIgnoreCase)
        {
            "/Account/Login",
            "/Account/Register",
            "/Account/ForgotPassword",
            "/Account/ResendEmailConfirmation",
            "/Account/ResetPassword",
            "/Account/ChangePassword",
            "/Account/DeleteAccount"
        };

        // key: "ip|page" -> attempts in the current window
        private static readonly ConcurrentDictionary<string, Counter> Counters = new();
        private static DateTime lastCleanupUtc = DateTime.UtcNow;

        private readonly RequestDelegate next;

        public AuthRateLimitMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (HttpMethods.IsPost(context.Request.Method))
            {
                // The page chosen by routing, not the URL text: "/Login/", "/LOGIN" etc. are the same page
                // and must share one counter (comparing the URL let "/Login/" skip the limit).
                var path = ProtectedPageOf(context);

                if (path != null)
                {
                    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    var now = DateTime.UtcNow;

                    CleanupIfNeeded(now);

                    var counter = Counters.AddOrUpdate(
                        $"{ip}|{path}",
                        _ => new Counter(now, 1),
                        (_, existing) => now - existing.WindowStartUtc >= Window
                            ? new Counter(now, 1)
                            : existing with { Count = existing.Count + 1 });

                    if (counter.Count > MaxAttemptsPerWindow)
                    {
                        var retryAfter = Window - (now - counter.WindowStartUtc);
                        context.Response.Headers["Retry-After"] = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();
                        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                        return;
                    }
                }
            }

            await next(context);
        }

        private static string? ProtectedPageOf(HttpContext context)
        {
            var area = context.GetRouteValue("area") as string;
            var page = context.GetRouteValue("page") as string;

            return string.Equals(area, "Identity", StringComparison.OrdinalIgnoreCase)
                   && page != null
                   && ProtectedPages.Contains(page)
                ? page
                : null;
        }

        private static void CleanupIfNeeded(DateTime now)
        {
            if (now - lastCleanupUtc < TimeSpan.FromMinutes(5))
            {
                return;
            }

            lastCleanupUtc = now;

            foreach (var entry in Counters)
            {
                if (now - entry.Value.WindowStartUtc >= Window)
                {
                    Counters.TryRemove(entry.Key, out _);
                }
            }
        }

        /// <summary>For tests: forget all counters.</summary>
        public static void Reset() => Counters.Clear();

        private record Counter(DateTime WindowStartUtc, int Count);
    }
}
