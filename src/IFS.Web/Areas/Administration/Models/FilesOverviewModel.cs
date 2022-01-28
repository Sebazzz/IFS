// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : FilesOverviewModel.cs
//  Project         : IFS.Web
// ******************************************************************************

using System.Collections.Generic;

using IFS.Web.Core.Upload;

namespace IFS.Web.Areas.Administration.Models;

public class FilesOverviewModel {
    public IList<UploadedFile> Files { get;}

    public FilesOverviewModel(IList<UploadedFile> files)
    {
        this.Files = files;
    }
}