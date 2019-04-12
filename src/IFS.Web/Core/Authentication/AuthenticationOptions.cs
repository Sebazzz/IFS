// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : AuthenticationOptions.cs
//  Project         : IFS.Web
// ******************************************************************************
namespace IFS.Web.Core.Authentication {
    using OpenIdConnect;
    using Static;

    public sealed class AuthenticationOptions {
        public StaticAuthenticationOptions Static { get; set; }

        public OpenIdConnectSettings OpenIdConnect { get; set; }

        public string LoginHelpText { get; set; }
    }
}