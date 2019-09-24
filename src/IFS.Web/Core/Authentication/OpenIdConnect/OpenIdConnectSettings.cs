// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : OpenIdConnectSettings.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core.Authentication.OpenIdConnect {
    #nullable disable
    public class OpenIdConnectSettings {
        public bool Enable { get; set; }

        public string ClientSecret { get; set; }
        public string ClientId { get; set; }

        public string MetadataAddress { get; set; }

        public OpenIdRoleClaims RoleClaims { get; set; }

        public OpenIdConnectClaimMapping ClaimMapping { get; set; }

        public string Authority { get; set; }
    }
}
