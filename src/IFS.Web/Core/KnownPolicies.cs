// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : KnownPolicies.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core {
    public static class KnownPolicies {
        public const string Upload = nameof(Upload);
    }

    public static class KnownAuthenticationScheme {
        public const string PassphraseScheme = "Cookie";
    }
}
