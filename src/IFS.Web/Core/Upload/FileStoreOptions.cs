// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : FileStoreOptions.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core.Upload;
#nullable disable
public class FileStoreOptions
{
    public string StorageDirectory { get; set; }

    public int MaximumFileSize { get; set; }
}