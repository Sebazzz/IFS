// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : KnownPolicies.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core {
    public static class KnownPolicies {
        public const string Upload = nameof(Upload);
        public const string Administration = nameof(Administration);
    }

    public static class KnownAuthenticationScheme {
        public const string PassphraseScheme= nameof(PassphraseScheme);
        public const string AdministrationScheme = nameof(AdministrationScheme);
    }
}
