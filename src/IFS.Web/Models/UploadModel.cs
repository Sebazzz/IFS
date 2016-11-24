// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : UploadModel.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Models {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using Core;
    using Core.Upload;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Rendering;

    public class UploadModel {
        public FileIdentifier FileIdentifier { get; set; }

        [Required(ErrorMessage = "Please select a file to upload")]
        [FileSizeValidation]
        [Display(Name = "Your file")]
        public IFormFile File { get; set; }
        
        [Display]
        public DateTime Expiration { get; set; }

        [Display(Name = "When will the file expiration start?")]
        public ExpirationMode ExpirationMode { get; set; }

        public long SuggestedFileSize { get; set; }
        public IEnumerable<SelectListItem> AvailableExpiration { get; set; }

    }

    public class UploadFileInProgressModel {
        public string FileName { get; set; }

        public FileIdentifier FileIdentifier { get; set; }
    }

    public class UploadProgressModel {
        public long Current { get; set; }
        public long Total { get; set; }

        public string FileName { get; set; }

        public string Performance { get; set; }

        public int Percent { get; set; }
    }
}
