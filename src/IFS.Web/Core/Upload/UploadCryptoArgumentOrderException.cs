// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : UploadCryptoArgumentOrderException.cs
//  Project         : IFS.Web
// ******************************************************************************
namespace IFS.Web.Core.Upload {
    using System;

    public class UploadCryptoArgumentOrderException : UploadCryptoException {
        public UploadCryptoArgumentOrderException() {}
        public UploadCryptoArgumentOrderException(string message) : base(message) {}
        public UploadCryptoArgumentOrderException(string message, Exception innerException) : base(message, innerException) {}
    }
}