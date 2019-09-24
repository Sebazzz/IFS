// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : LoginViewModel.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Areas.Administration.Models {
    using System.ComponentModel.DataAnnotations;

    public sealed class LoginModel {
        public string? ReturnUrl { get; set; }

        [Required]
        [Display(Name = "User name")]
        public string? UserName { get; set; }

        [Required]
        [Display]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
