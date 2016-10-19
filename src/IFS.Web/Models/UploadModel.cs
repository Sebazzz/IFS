// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : UploadModel.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Models {
    using System.ComponentModel.DataAnnotations;

    using Microsoft.AspNetCore.Http;

    public class UploadModel {
        public FileIdentifier FileIdentifier { get; set; }

        [Required(ErrorMessage = "Please select a file to upload")]
        public IFormFile File { get; set; }
    }

    public class UploadFileInProgressModel {
        public string FileName { get; set; }

        public FileIdentifier FileIdentifier { get; set; }
    }
}
