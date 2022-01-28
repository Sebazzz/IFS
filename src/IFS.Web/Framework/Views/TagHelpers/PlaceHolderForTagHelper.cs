// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : PlaceHolderForTagHelper.cs
//  Project         : IFS.Web
// ******************************************************************************

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace IFS.Web.Framework.Views.TagHelpers;

[HtmlTargetElement("input", Attributes = PlaceHolderForAttribute)]
public sealed class PlaceHolderForTagHelper : TagHelper {
    private const string PlaceHolderForAttribute = "placeholder-for";

    [HtmlAttributeName(PlaceHolderForAttribute)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    public ModelExpression Target { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

    public override void Process(TagHelperContext context, TagHelperOutput output) {
        output.Attributes.SetAttribute("placeholder", this.Target.Metadata.DisplayName ?? this.Target.Metadata.PropertyName);
    }
}