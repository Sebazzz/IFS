// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : AuthenticationBuilderExtensions.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core.Authentication.Static {
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

    internal static class AuthenticationBuilderExtensions {
        public static AuthenticationBuilder AddCookie(this AuthenticationBuilder authBuilder, string authenticationScheme, PathString loginPath) {
            return authBuilder.AddCookie(authenticationScheme, 
                opt => {
                opt.LoginPath = new PathString(loginPath);
                opt.AccessDeniedPath = new PathString("/error/accessDenied");
                opt.ReturnUrlParameter = "returnUrl";
            });
        }
    }
}
