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
        public AdministrationAuthenticationOptions Administration { get; set; }

        public string LoginHelpText { get; set; }
    }

    public sealed class AdministrationAuthenticationOptions {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class AuthenticationProvider : IAuthenticationProvider, IAdministrationAuthenticationProvider {
        private readonly IOptions<AuthenticationOptions> _options;

        public AuthenticationProvider(IOptions<AuthenticationOptions> options) {
            this._options = options;
        }

        public bool IsValidPassphrase(string passphrase) {
            return this._options.Value?.Passphrase == passphrase;
        }

        public bool IsValidCredentials(string userName, string password) {
            var options = this._options.Value?.Administration;

            if (options == null) {
                return false;
            }

            return userName == options.UserName &&
                   password == options.Password;
        }
    }

    public interface IAuthenticationProvider {
        bool IsValidPassphrase(string passphrase);
    }

    public interface IAdministrationAuthenticationProvider {
        bool IsValidCredentials(string userName, string password);
    }
}
