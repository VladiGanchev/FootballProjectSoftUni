using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FootballProjectSoftUni.Extensions
{
    /// <summary>
    /// Puts this request's Content-Security-Policy nonce on every &lt;script&gt; tag of our pages.
    /// The policy (Program.cs) only runs scripts that carry it, so a script somebody manages to inject
    /// into a page is refused by the browser.
    /// </summary>
    [HtmlTargetElement("script")]
    public class ScriptNonceTagHelper : TagHelper
    {
        public const string ItemKey = "CspNonce";

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; } = null!;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (ViewContext.HttpContext.Items[ItemKey] is string nonce)
            {
                output.Attributes.SetAttribute("nonce", nonce);
            }
        }
    }
}
