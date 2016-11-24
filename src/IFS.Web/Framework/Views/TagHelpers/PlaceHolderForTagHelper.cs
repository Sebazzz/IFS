// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : PlaceHolderForTagHelper.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Framework.Views.TagHelpers {
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.AspNetCore.Razor.TagHelpers;

    [HtmlTargetElement("input", Attributes = PlaceHolderForAttribute)]
    public sealed class PlaceHolderForTagHelper : TagHelper {
        private const string PlaceHolderForAttribute = "placeholder-for";

        [HtmlAttributeName(PlaceHolderForAttribute)]
        public ModelExpression Target { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output) {
            output.Attributes.SetAttribute("placeholder", this.Target.Metadata.DisplayName ?? this.Target.Metadata.PropertyName);
        }
    }
}
