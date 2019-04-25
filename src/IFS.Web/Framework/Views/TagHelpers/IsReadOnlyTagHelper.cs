// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : IsReadOnlyTagHelper.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Framework.Views.TagHelpers {
    using Microsoft.AspNetCore.Razor.TagHelpers;

    /// <summary>
    /// Workaround tag helper to render no-value attribute "readonly" conditionally, see: https://stackoverflow.com/q/55837409/646215
    /// </summary>
    [HtmlTargetElement("input", Attributes = IsReadOnlyAttribute)]
    public sealed class IsReadOnlyTagHelper : TagHelper {
        private const string IsReadOnlyAttribute = "is-readonly";

        [HtmlAttributeName(IsReadOnlyAttribute)]
        public bool IsReadOnly { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output) {
            if (this.IsReadOnly) {
                output.Attributes.SetAttribute("readonly", null);
            }
        }
    }
}