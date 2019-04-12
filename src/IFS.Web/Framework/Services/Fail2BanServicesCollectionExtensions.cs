// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : Fail2BanServicesCollectionExtensions.cs
//  Project         : IFS.Web
// ******************************************************************************

using IFS.Web.Core.Authentication;
using IFS.Web.Framework.Middleware.Fail2Ban;
using Microsoft.Extensions.DependencyInjection;

namespace IFS.Web.Framework.Services
{
    internal static class Fail2BanServicesCollectionExtensions
    {
        public static void AddFail2Ban(this IServiceCollection services)
        {
            services.AddOptions<Fail2BanOptions>("Fail2Ban");

            services.AddSingleton<IFail2Ban, Fail2Ban>();

            services.AddSingleton<Fail2BanRecordMiddleware>();
        }
    }
}