namespace IFS.Web.Core.Upload {
    using System;

    public class UploadFailedException : Exception {
        public UploadFailedException() {}
        public UploadFailedException(string message) : base(message) {}
        public UploadFailedException(string message, Exception innerException) : base(message, innerException) {}
    }
}