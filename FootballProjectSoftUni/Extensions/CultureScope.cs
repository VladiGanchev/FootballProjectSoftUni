using System.Globalization;

namespace FootballProjectSoftUni.Extensions
{
    /// <summary>
    /// Builds texts in another user's language (notifications and e-mails are stored / sent as text,
    /// so they are written in the language of the person who receives them).
    /// </summary>
    public static class CultureScope
    {
        public static T Run<T>(string culture, Func<T> build)
        {
            var previousCulture = CultureInfo.CurrentCulture;
            var previousUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                var target = new CultureInfo(culture == "en" ? "en" : "bg");
                CultureInfo.CurrentCulture = target;
                CultureInfo.CurrentUICulture = target;

                return build();
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
                CultureInfo.CurrentUICulture = previousUiCulture;
            }
        }
    }
}
