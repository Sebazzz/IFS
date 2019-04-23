// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : UploadErrorsModel.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Models {
    public class UploadErrorsModel {
        public string[] Errors { get; set; }

        public static UploadErrorsModel CreateFromMessage(string message) => new UploadErrorsModel {
            Errors = new[] {message}
        };
    }
}
