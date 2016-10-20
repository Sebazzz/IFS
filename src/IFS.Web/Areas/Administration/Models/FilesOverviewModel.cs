// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : FilesOverviewModel.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Areas.Administration.Models {
    using System.Collections.Generic;

    using Core.Upload;

    public class FilesOverviewModel {
        public IList<UploadedFile> Files { get; set; }
    }
}
