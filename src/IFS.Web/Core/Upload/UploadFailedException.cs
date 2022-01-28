// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : UploadFailedException.cs
//  Project         : IFS.Web
// ******************************************************************************

using System;

namespace IFS.Web.Core.Upload {
    public class UploadFailedException : Exception {
        public UploadFailedException() { }
        public UploadFailedException(string message) : base(message) { }
        public UploadFailedException(string message, Exception innerException) : base(message, innerException) { }
    }
}