using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace FootballProjectSoftUni.Extensions
{
    /// <summary>
    /// Reads money amounts whatever the decimal separator. Number fields in the browser always send
    /// "250.5", while someone typing in Bulgarian may write "250,5". The default binder accepts only the
    /// separator of the current language, so on the Bulgarian site "250.5" could not be read.
    /// </summary>
    public class FlexibleDecimalModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueResult);

            var metadata = bindingContext.ModelMetadata;
            var text = valueResult.FirstValue?.Trim();

            if (string.IsNullOrEmpty(text))
            {
                if (metadata.IsReferenceOrNullableType)
                {
                    bindingContext.Result = ModelBindingResult.Success(null);
                }
                else
                {
                    bindingContext.ModelState.TryAddModelError(bindingContext.ModelName,
                        metadata.ModelBindingMessageProvider.ValueMustNotBeNullAccessor(valueResult.FirstValue ?? string.Empty));
                }

                return Task.CompletedTask;
            }

            if (TryParse(text, out var value))
            {
                bindingContext.Result = ModelBindingResult.Success(value);
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName,
                    metadata.ModelBindingMessageProvider.AttemptedValueIsInvalidAccessor(text, metadata.GetDisplayName()));
            }

            return Task.CompletedTask;
        }

        public static bool TryParse(string text, out decimal value)
        {
            var s = text.Replace(" ", string.Empty).Replace(" ", string.Empty);
            var lastComma = s.LastIndexOf(',');
            var lastDot = s.LastIndexOf('.');

            if (lastComma >= 0 && lastDot >= 0)
            {
                // Both: the last one separates the decimals, the other one groups thousands.
                s = lastComma > lastDot
                    ? s.Replace(".", string.Empty).Replace(',', '.')
                    : s.Replace(",", string.Empty);
            }
            else if (lastComma >= 0)
            {
                s = s.Replace(',', '.');
            }

            return decimal.TryParse(s, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value);
        }
    }

    public class FlexibleDecimalModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context) =>
            context.Metadata.UnderlyingOrModelType == typeof(decimal)
                ? new FlexibleDecimalModelBinder()
                : null;
    }
}
