using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace FootballProjectSoftUni.Extensions
{
    /// <summary>
    /// The errors of registration, password change and password reset (password rules, taken e-mail,
    /// expired link...) in the visitor's language instead of the built-in English texts.
    /// </summary>
    public class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
    {
        private readonly IStringLocalizer<SharedResource> localizer;

        public LocalizedIdentityErrorDescriber(IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
        }

        private IdentityError Error(string code, params object[] args) =>
            new IdentityError { Code = code, Description = localizer["Id" + code, args].Value };

        public override IdentityError DefaultError() => Error(nameof(DefaultError));

        public override IdentityError ConcurrencyFailure() => Error(nameof(ConcurrencyFailure));

        public override IdentityError PasswordMismatch() => Error(nameof(PasswordMismatch));

        public override IdentityError InvalidToken() => Error(nameof(InvalidToken));

        // The user name is the e-mail on this site, so both speak about the e-mail.
        public override IdentityError InvalidUserName(string? userName) => Error(nameof(InvalidUserName), userName ?? string.Empty);

        public override IdentityError InvalidEmail(string? email) => Error(nameof(InvalidEmail), email ?? string.Empty);

        public override IdentityError DuplicateUserName(string userName) => Error(nameof(DuplicateUserName), userName);

        public override IdentityError DuplicateEmail(string email) => Error(nameof(DuplicateEmail), email);

        public override IdentityError PasswordTooShort(int length) => Error(nameof(PasswordTooShort), length);

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => Error(nameof(PasswordRequiresUniqueChars), uniqueChars);

        public override IdentityError PasswordRequiresNonAlphanumeric() => Error(nameof(PasswordRequiresNonAlphanumeric));

        public override IdentityError PasswordRequiresDigit() => Error(nameof(PasswordRequiresDigit));

        public override IdentityError PasswordRequiresLower() => Error(nameof(PasswordRequiresLower));

        public override IdentityError PasswordRequiresUpper() => Error(nameof(PasswordRequiresUpper));
    }
}
