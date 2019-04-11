// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : TransitPasswordProtector.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core {
    using System;
    using Microsoft.AspNetCore.DataProtection;

    public interface ITransitPasswordProtector {
        string Protect(string password);
        string Unprotect(string protectedPassword);
    }

    public sealed class TransitPasswordProtector : ITransitPasswordProtector {
        private const string Purpose = "Password-in-transit";
        private const int ProtectionTimeLimitMinutes = 30;

        private readonly ITimeLimitedDataProtector _dataProtector;

        public TransitPasswordProtector(IDataProtectionProvider dataProtectionProvider) {
            this._dataProtector = dataProtectionProvider.CreateProtector(Purpose).ToTimeLimitedDataProtector();
        }

        public string Protect(string password) {
            DateTimeOffset expiration = DateTimeOffset.UtcNow.AddMinutes(ProtectionTimeLimitMinutes);

            return this._dataProtector.Protect(password, expiration);
        }

        public string Unprotect(string protectedPassword) {
            return this._dataProtector.Unprotect(protectedPassword, out _);
        }
    }
}
