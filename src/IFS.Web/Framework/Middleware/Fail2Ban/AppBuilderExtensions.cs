// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : AppBuilderExtensions.cs
//  Project         : IFS.Web
// ******************************************************************************

using Microsoft.AspNetCore.Builder;

namespace IFS.Web.Framework.Middleware.Fail2Ban
{
    internal static class AppBuilderExtensions
    {
        public static void UseFail2BanRecording(this IApplicationBuilder app)
        {
            app.UseMiddleware<Fail2BanRecordMiddleware>();
        }
    }
}
