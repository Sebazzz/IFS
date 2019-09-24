// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : DownloadPasswordModel.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Models {
    using System.ComponentModel.DataAnnotations;

    public class DownloadPasswordModel {
        [Required]
        [DataType(DataType.Password)]
        [StringLength(1204)] // DoS protection
        public string? Password { get; set; }
    }
}
