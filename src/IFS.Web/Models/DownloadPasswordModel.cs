// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : DownloadPasswordModel.cs
//  Project         : IFS.Web
// ******************************************************************************

using System.ComponentModel.DataAnnotations;

namespace IFS.Web.Models;

public class DownloadPasswordModel {
    [Required]
    [DataType(DataType.Password)]
    [StringLength(1204)] // DoS protection
    public string? Password { get; set; }
}