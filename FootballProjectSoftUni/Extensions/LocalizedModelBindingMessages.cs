using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace FootballProjectSoftUni.Extensions
{
    /// <summary>
    /// The built-in form errors ("The value '' is invalid." ...) are English only;
    /// these come from the resources, in the language of the site.
    /// </summary>
    public class LocalizedModelBindingMessages : IConfigureOptions<MvcOptions>
    {
        private readonly IStringLocalizer localizer;

        public LocalizedModelBindingMessages(IStringLocalizerFactory factory)
        {
            localizer = factory.Create(typeof(SharedResource));
        }

        public void Configure(MvcOptions options)
        {
            var messages = options.ModelBindingMessageProvider;

            messages.SetValueMustNotBeNullAccessor(_ => localizer["MBRequired"].Value);
            messages.SetMissingBindRequiredValueAccessor(_ => localizer["MBRequired"].Value);
            messages.SetMissingKeyOrValueAccessor(() => localizer["MBRequired"].Value);
            messages.SetMissingRequestBodyRequiredValueAccessor(() => localizer["MBRequired"].Value);
            messages.SetAttemptedValueIsInvalidAccessor((value, _) => localizer["MBInvalidValue", value].Value);
            messages.SetNonPropertyAttemptedValueIsInvalidAccessor(value => localizer["MBInvalidValue", value].Value);
            messages.SetValueIsInvalidAccessor(value => localizer["MBInvalidValue", value].Value);
            messages.SetUnknownValueIsInvalidAccessor(_ => localizer["MBInvalid"].Value);
            messages.SetNonPropertyUnknownValueIsInvalidAccessor(() => localizer["MBInvalid"].Value);
            messages.SetValueMustBeANumberAccessor(_ => localizer["MBNumber"].Value);
            messages.SetNonPropertyValueMustBeANumberAccessor(() => localizer["MBNumber"].Value);
        }
    }
}
