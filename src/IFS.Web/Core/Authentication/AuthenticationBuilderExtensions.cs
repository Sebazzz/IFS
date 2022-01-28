// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : AuthenticationBuilderExtensions.cs
//  Project         : IFS.Web
// ******************************************************************************

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using IFS.Web.Core.Authentication.OpenIdConnect;
using IFS.Web.Core.Authentication.Static;

namespace IFS.Web.Core.Authentication;

internal static class AuthenticationBuilderExtensions {
    public static AuthenticationBuilder AddFromSettings(this AuthenticationBuilder authBuilder, IConfiguration configuration) {
        AuthenticationOptions authOptions = configuration.GetSection("Authentication").Get<AuthenticationOptions>();

        if (authOptions.OpenIdConnect?.Enable == true) {
            authBuilder.AddOpenIdConnectFromSettings(
                    KnownAuthenticationScheme.OpenIdConnect.PassphraseScheme, 
                    KnownAuthenticationScheme.PassphraseScheme,
                    "upload",
                    configuration)
                .AddOpenIdConnectFromSettings(
                    KnownAuthenticationScheme.OpenIdConnect.AdministrationScheme, 
                    KnownAuthenticationScheme.AdministrationScheme, 
                    "admin",
                    configuration);
        }

        return authBuilder.AddCookie(KnownAuthenticationScheme.PassphraseScheme, "/authenticate/login")
            .AddCookie(KnownAuthenticationScheme.AdministrationScheme, "/administration/authenticate/login");
    }
}