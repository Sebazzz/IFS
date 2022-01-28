// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : DownloadStreamFactory.cs
//  Project         : IFS.Web
// ******************************************************************************

using System.IO;
using IFS.Web.Core.Upload;

namespace IFS.Web.Core.Download {
    internal static class DownloadStreamFactory {
        public static Stream GetDownloadStream(UploadedFile file, string? password)
        {
            return password == null ? file.GetStream() : CryptoStreamWrapper.Create(file, password);
        }
    }
}