// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : AuthenticationBuilderExtensions.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core.Authentication.OpenIdConnect {
    using System;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.OAuth.Claims;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json.Linq;

    internal static class AuthenticationBuilderExtensions {
        public static AuthenticationBuilder AddOpenIdConnectFromSettings(this AuthenticationBuilder authBuilder, string authenticationScheme, string mappedAuthenticationScheme, IConfiguration configuration) {
            OpenIdConnectSettings openIdConnectSettings = configuration.GetSection("Authentication").GetSection("OpenIdConnect").Get<OpenIdConnectSettings>();

            if (openIdConnectSettings.Enable == false) {
                return authBuilder;
            }

            return authBuilder.AddOpenIdConnect(
                authenticationScheme,
                opts => {
                    opts.Authority = openIdConnectSettings.Authority;
                    opts.ClientId = openIdConnectSettings.ClientId;
                    opts.ClientSecret = openIdConnectSettings.ClientSecret;
                    opts.MetadataAddress = openIdConnectSettings.MetadataAddress;
                    opts.SignedOutCallbackPath = "/oidc_signout";
                    opts.CallbackPath = "/oidc_signin";
                    
                    opts.SignInScheme = mappedAuthenticationScheme;
                    opts.SignOutScheme = mappedAuthenticationScheme;

                    opts.AuthenticationMethod = OpenIdConnectRedirectBehavior.FormPost;

                    opts.ClaimActions.Add(
                        new ConsolidateRoleClaimAction(
                            nameof(openIdConnectSettings.RoleClaims.Administrator), 
                            openIdConnectSettings.RoleClaims.Administrator));
                }
            );
        }

        private sealed class ConsolidateRoleClaimAction : ClaimAction {
            private readonly string _roleName;
            private readonly OpenIdRoleClaim _roleMapping;

            public ConsolidateRoleClaimAction(string roleName, OpenIdRoleClaim roleMapping) : base(ClaimTypes.Role, nameof(String)) {
                this._roleMapping = roleMapping;
                this._roleName = roleName;
            }

            public override void Run(JObject userData, ClaimsIdentity identity, string issuer) {
                Claim originalClaim = identity.FindFirst(x => x.Type == this._roleMapping.ClaimType && x.Value == this._roleMapping.Value);
                if (originalClaim == null) {
                    return;
                }

                Claim replaceClaim = new Claim(
                    this.ClaimType,
                    this._roleName,
                    this.ValueType,
                    originalClaim.Issuer
                );
                
                identity.RemoveClaim(originalClaim);
                identity.AddClaim(replaceClaim);
            }
        }
    }
}
