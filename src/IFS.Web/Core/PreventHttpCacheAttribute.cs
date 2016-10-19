// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : PreventHttpCacheAttribute.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core {
    using System;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Net.Http.Headers;

    public sealed class PreventHttpCacheAttribute : ActionFilterAttribute {
        public override void OnResultExecuting(ResultExecutingContext context) {
            HttpResponse response = context.HttpContext.Response;

            response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue {
                NoStore = true,
                MaxAge = TimeSpan.Zero
            };
        }
    }
}
