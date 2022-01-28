// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : UploadModel.cs
//  Project         : IFS.Web
// ******************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using IFS.Web.Core;
using IFS.Web.Core.Upload;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IFS.Web.Models;

public class UploadLinkModel : UploadModelBase {
        
}

public class UploadModelBase : IContactInformationModel {
    public FileIdentifier FileIdentifier { get; set; }
    [Display]
    public DateTime Expiration { get; set; }
    public IEnumerable<SelectListItem>? AvailableExpiration { get; set; }

    public ContactInformation? Sender { get; set; }

    public bool IsSenderInformationPrefilled { get; set; }
}

public class UploadModel : UploadModelBase {

    [Required(ErrorMessage = "Please select a file to upload")]
    [FileSizeValidation]
    [Display(Name = "Your file")]
    public IFormFile? File { get; set; }

    public long SuggestedFileSize { get; set; }

    public bool IsReservation { get; set; }

    [StringLength(512)]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    public bool EnablePasswordProtection { get;set; }
}

public class UploadFileInProgressModel {
    public string? FileName { get; set; }

    public FileIdentifier FileIdentifier { get; set; }
}

public class UploadProgressModel {
    public long Current { get; set; }
    public long Total { get; set; }

    public string? FileName { get; set; }

    public string? Performance { get; set; }

    public int Percent { get; set; }
}