using Microsoft.AspNetCore.Mvc.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FootballProjectSoftUni.Extensions
{
    /// <summary>
    /// Marks requests rejected because the form's security token was missing or expired (the page was open
    /// too long, the user signed in/out in another tab...). The error page then explains that instead of a
    /// generic "bad request".
    /// </summary>
    public class AntiforgeryFailureMarker : IAlwaysRunResultFilter
    {
        public const string ItemKey = "AntiforgeryFailed";

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is IAntiforgeryValidationFailedResult)
            {
                context.HttpContext.Items[ItemKey] = true;
            }
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }
    }
}
