// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : PreventHttpCacheAttribute.cs
//  Project         : IFS.Web
// ******************************************************************************

using System;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace IFS.Web.Core {
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
