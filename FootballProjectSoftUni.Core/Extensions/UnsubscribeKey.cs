using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;
using System.Text;

namespace FootballProjectSoftUni.Core.Extensions
{
    /// <summary>
    /// The key in the "unsubscribe" link of the e-mails about new tournaments. It turns those e-mails off without
    /// signing in, and nobody can make one for somebody else's account. It comes from the account's security stamp,
    /// so it keeps working after the site restarts and stops working when the password is changed.
    /// </summary>
    public static class UnsubscribeKey
    {
        /// <returns>The key, or null for an account without a security stamp.</returns>
        public static string? For(string userId, string? securityStamp)
        {
            if (string.IsNullOrEmpty(securityStamp))
            {
                return null;
            }

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(securityStamp));
            return WebEncoders.Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes("new-tournament-emails:" + userId)));
        }

        public static bool IsValid(string userId, string? securityStamp, string? key)
        {
            var expected = For(userId, securityStamp);

            return expected != null
                && key != null
                && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(key));
        }
    }
}
