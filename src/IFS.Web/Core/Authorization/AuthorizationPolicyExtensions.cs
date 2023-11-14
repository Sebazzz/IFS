using IFS.Web.Core.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace IFS.Web.Core.Authorization;

internal static class AuthorizationPolicyExtensions {
    public static void AddUploadPolicy(this AuthorizationOptions authorizationOptions, IConfiguration configuration) {
        AuthenticationOptions authOptions = configuration.GetSection("Authentication").Get<AuthenticationOptions>();

        authorizationOptions.AddPolicy(
            KnownPolicies.Upload,
            b => {
                b.AddAuthenticationSchemes(KnownAuthenticationScheme.PassphraseScheme)
                    .AddRequirements(new RestrictedUploadRequirement())
                    .RequireAuthenticatedUser();

                if (authOptions is { OpenIdConnect.Enable: true }) {
                    // No extra requirements
                } else {
                    b.RequireUserName(KnownPolicies.Upload);
                }
            }
        );
    }

    public static void AddAdministrationPolicy(this AuthorizationOptions authorizationOptions, IConfiguration configuration) {
        AuthenticationOptions authOptions = configuration.GetSection("Authentication").Get<AuthenticationOptions>();

        authorizationOptions.AddPolicy(
            KnownPolicies.Administration,
            b => {
                b.AddAuthenticationSchemes(KnownAuthenticationScheme.AdministrationScheme)
                    .RequireAuthenticatedUser();

                if (authOptions is null) return;
                
                if (authOptions is { OpenIdConnect.Enable: true }) {
                    b.RequireRole(KnownRoles.Administrator);
                } else {
                    b.RequireUserName(authOptions.Static.Administration.UserName);
                }
            }
        );
    }
}