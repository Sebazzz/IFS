// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : UploadModel.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Models {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using Core;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Rendering;

    public class NewUploadModel : UploadModel {
        public IEnumerable<SelectListItem> AvailableExpiration { get; set; }
    }

    public class UploadModel {
        public FileIdentifier FileIdentifier { get; set; }

        [Required(ErrorMessage = "Please select a file to upload")]
        [FileSizeValidation]
        public IFormFile File { get; set; }

        public DateTime Expiration { get; set; }

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
