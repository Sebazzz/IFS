// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : OpenIdRoleClaim.cs
//  Project         : IFS.Web
// ******************************************************************************
namespace IFS.Web.Core.Authentication.OpenIdConnect {
    #nullable disable
    public class OpenIdRoleClaim {
        public string ClaimType { get; set; }
        public string Value { get; set; }
    }
}