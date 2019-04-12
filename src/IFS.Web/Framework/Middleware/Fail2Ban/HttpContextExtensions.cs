// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : HttpContextExtensions.cs
//  Project         : IFS.Web
// ******************************************************************************

using Microsoft.AspNetCore.Http;

namespace IFS.Web.Framework.Middleware.Fail2Ban
{
    internal static class HttpContextExtensions
    {
        public static void RecordFail2BanSuccess(this HttpContext httpContext) => httpContext.RecordFail2BanResult(true);
        public static void RecordFail2BanFailure(this HttpContext httpContext) => httpContext.RecordFail2BanResult(false);

        private static void RecordFail2BanResult(this HttpContext httpContext, bool isSuccess)
        {
            httpContext.Features.Set(new Fail2BanFeature(isSuccess));
        }
    }
}