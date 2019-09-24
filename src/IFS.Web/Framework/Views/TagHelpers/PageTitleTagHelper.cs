// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : PageTitleTagHelper.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Framework.Views.TagHelpers {
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Html;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.AspNetCore.Razor.TagHelpers;

    /// <summary>
    /// Shows a default title
    /// </summary>
    [HtmlTargetElement("ifs-page-title")]
    public sealed class PageTitleTagHelper : TagHelper {
        [HtmlAttributeNotBound]
        [ViewContext]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public ViewContext ViewContext { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        [SuppressMessage("ReSharper", "MustUseReturnValue", Justification = "IHtmlBuilder returns itself ('this' instance)")]
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) {
            TagHelperContent tagContents = await output.GetChildContentAsync();
            string tagContentsString = tagContents.GetContent(NullHtmlEncoder.Default);

            TagBuilder innerBuilder = new TagBuilder("small");
            innerBuilder.AddCssClass("text-muted");
            innerBuilder.InnerHtml.Append(tagContentsString);

            output.TagName = "h1";
            output.Content.Clear()
                          .AppendLine("Internet File System")
                          .AppendHtml(innerBuilder);
        }
    }
}
