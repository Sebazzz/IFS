// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : AuthenticationProvider.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core.Authentication {
    using Microsoft.Extensions.Options;

    public sealed class AuthenticationOptions {
        public string Passphrase { get; set; }
    }

    public class AuthenticationProvider : IAuthenticationProvider {
        private readonly IOptions<AuthenticationOptions> _options;

        public AuthenticationProvider(IOptions<AuthenticationOptions> options) {
            this._options = options;
        }

        public bool IsValidPassphrase(string passphrase) {
            return this._options.Value?.Passphrase == passphrase;
        }
    }

    public interface IAuthenticationProvider {

        bool IsValidPassphrase(string passphrase);
    }
}
