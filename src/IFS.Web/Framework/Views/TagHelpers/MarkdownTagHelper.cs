// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : MarkdownTagHelper.cs
//  Project         : IFS.Web
// ******************************************************************************

using System.Threading.Tasks;

using Microsoft.AspNetCore.Razor.TagHelpers;

namespace IFS.Web.Framework.Views.TagHelpers;

[HtmlTargetElement("markdown", TagStructure = TagStructure.NormalOrSelfClosing)]
public sealed class MarkdownTagHelper : TagHelper {
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) {
        TagHelperContent content = await output.GetChildContentAsync(true, NullHtmlEncoder.Default);

        string markDown = content.GetContent();
        string html = Markdig.Markdown.ToHtml(markDown);

        output.Content = output.Content.SetHtmlContent(html);
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
    }
        
}