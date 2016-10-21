// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : AllowLargeFileAttribute.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core {
    using System;

    using Humanizer;

    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    using Upload;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class AllowLargeFileAttribute : Attribute, IAuthorizationFilter, IOrderedFilter {
        public int Order { get; set; }

        public void OnAuthorization(AuthorizationFilterContext context) {
            var features = context.HttpContext.Features;
            var formFeature = features.Get<IFormFeature>();

            if (formFeature?.Form == null) {
                // Request form has not been read yet, so set the limits
                var config = context.HttpContext.RequestServices.GetRequiredService<IOptions<FileStoreOptions>>()?.Value;

                if (config == null) {
                    return;
                }

                FormOptions options = new FormOptions {
                    BufferBody = false,
                    MultipartBodyLengthLimit = (long) config.MaximumFileSize.Megabytes().Bytes
                };

                features.Set<IFormFeature>(new FormFeature(context.HttpContext.Request, options));
            }
        }
    }
}
