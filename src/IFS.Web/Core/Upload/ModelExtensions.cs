// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : ModelExtensions.cs
//  Project         : IFS.Web
// ******************************************************************************
namespace IFS.Web.Core.Upload {
    internal static class ModelExtensions {
        public static bool HasDownloadSecurity(this UploadedFile uploadModel) {
            return uploadModel.Metadata.DownloadSecurity != null;
        }
    }
}
