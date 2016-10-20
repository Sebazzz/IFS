// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : LoginViewModel.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Areas.Administration.Models {
    using System.ComponentModel.DataAnnotations;

    public class LoginModel {
        public string ReturnUrl { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
