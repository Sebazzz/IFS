// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : DownloadStreamFactory.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core.Download {
    using System.IO;
    using Upload;

    internal static class DownloadStreamFactory {
        public static Stream GetDownloadStream(UploadedFile file, string? password)
        {
            return password == null ? file.GetStream() : CryptoStreamWrapper.Create(file, password);
        }
    }
}