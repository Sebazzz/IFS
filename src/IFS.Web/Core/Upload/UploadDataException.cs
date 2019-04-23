// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : UploadDataException.cs
//  Project         : IFS.Web
// ******************************************************************************
namespace IFS.Web.Core.Upload {
    using System;

    public class UploadCryptoException : Exception {
        public UploadCryptoException() {}
        public UploadCryptoException(string message) : base(message) {}
        public UploadCryptoException(string message, Exception innerException) : base(message, innerException) {}
    }
}