// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : StaticAuthenticationOptions.cs
//  Project         : IFS.Web
// ******************************************************************************
namespace IFS.Web.Core.Authentication.Static {
    public sealed class StaticAuthenticationOptions {
        public string Passphrase { get; set; }
        public AdministrationAuthenticationOptions Administration { get; set; }
    }
}