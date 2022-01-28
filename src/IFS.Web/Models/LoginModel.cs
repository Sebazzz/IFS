// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : LoginViewModel.cs
//  Project         : IFS.Web
// ******************************************************************************

using System.ComponentModel.DataAnnotations;

namespace IFS.Web.Models;

public sealed class LoginModel {
    public string? ReturnUrl { get; set; }

    [DataType(DataType.Password)]
    public string? Passphrase { get; set; }

    public string? HelpText { get; set; }
}