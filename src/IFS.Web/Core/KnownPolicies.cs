// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : KnownPolicies.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core {
    using System.Security.Claims;

    public static class KnownPolicies {
        public const string Upload = nameof(Upload);
        public const string Administration = nameof(Administration);
    }

    public static class KnownAuthenticationScheme {
        public const string PassphraseScheme= nameof(PassphraseScheme);
        public const string AdministrationScheme = nameof(AdministrationScheme);

        public static class OpenIdConnect {
            public const string PassphraseScheme = nameof(OpenIdConnect) + nameof(PassphraseScheme);
            public const string AdministrationScheme =  nameof(OpenIdConnect) +nameof(AdministrationScheme);
        }
    }

    public static class KnownRoles {
        public const string Administrator = nameof(Administrator);
    }

    public static class KnownClaims {
        public const string RestrictionId = ClaimTypes.PrimarySid;
    }
}
