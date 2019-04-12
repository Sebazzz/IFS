namespace IFS.Web.Core.Authorization {
    using Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Configuration;

    internal static class AuthorizationPolicyExtensions {
        public static void AddUploadPolicy(this AuthorizationOptions authorizationOptions, IConfiguration configuration) {
            AuthenticationOptions authOptions = configuration.GetSection("Authentication").Get<AuthenticationOptions>();

            authorizationOptions.AddPolicy(
                KnownPolicies.Upload,
                b => {
                    b.AddAuthenticationSchemes(KnownAuthenticationScheme.PassphraseScheme)
                     .AddRequirements(new RestrictedUploadRequirement())
                     .RequireAuthenticatedUser();

                    if (authOptions.OpenIdConnect?.Enable == true) {
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

                    if (authOptions.OpenIdConnect?.Enable == true) {
                        b.RequireRole(nameof(authOptions.OpenIdConnect.RoleClaims.Administrator));
                    } else {
                        b.RequireUserName(authOptions.Static.Administration.UserName);
                    }
                }
            );
        }
    }
}
