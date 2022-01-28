// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : AuthenticationOptions.cs
//  Project         : IFS.Web
// ******************************************************************************
using IFS.Web.Core.Authentication.OpenIdConnect;
using IFS.Web.Core.Authentication.Static;

namespace IFS.Web.Core.Authentication;
#nullable disable
public sealed class AuthenticationOptions {
    public StaticAuthenticationOptions Static { get; set; }

    public OpenIdConnectSettings OpenIdConnect { get; set; }

#nullable enable
    public string? LoginHelpText { get; set; }
}