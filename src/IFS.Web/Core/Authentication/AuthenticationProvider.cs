// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : AuthenticationProvider.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core.Authentication {
    using Microsoft.Extensions.Options;
    using Static;

    public class AuthenticationProvider : IAuthenticationProvider, IAdministrationAuthenticationProvider {
        private readonly StaticAuthenticationOptions _options;

        public AuthenticationProvider(IOptions<AuthenticationOptions> options) {
            this._options = options.Value.Static;
        }

        public bool IsValidPassphrase(string? passphrase) {
            return this._options?.Passphrase == passphrase;
        }

        public bool IsValidCredentials(string? userName, string? password) {
            var options = this._options?.Administration;

            if (options == null) {
                return false;
            }

            return userName == options.UserName &&
                   password == options.Password;
        }
    }

    public interface IAuthenticationProvider {
        bool IsValidPassphrase(string? passphrase);
    }

    public interface IAdministrationAuthenticationProvider {
        bool IsValidCredentials(string? userName, string? password);
    }
}
