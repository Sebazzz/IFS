// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : LoginViewModel.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Models {
    using System.ComponentModel.DataAnnotations;

    public sealed class LoginModel {
        public string ReturnUrl { get; set; }

        [DataType(DataType.Password)]
        public string Passphrase { get; set; }

        public string HelpText { get; set; }
    }
}
