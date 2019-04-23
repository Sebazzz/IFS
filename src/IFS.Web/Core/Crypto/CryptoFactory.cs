// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : CryptoFactory.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core.Crypto {
    using System.Security.Cryptography;
    using Microsoft.AspNetCore.Cryptography.KeyDerivation;

    internal static class CryptoFactory {
        public const int KeySize = 128;
        public const int KeyByteSize = KeySize / 8;

        public static Aes CreateCrypto(string password) {
            return CreateCrypto(password, null);
        }

        public static Aes CreateCrypto(string password, byte[] iv) {
            Aes crypto = new AesManaged();

            if (iv != null) {
                crypto.IV = iv;
            }

            crypto.KeySize = KeySize;
            crypto.Key = KeyDerivation.Pbkdf2(password, crypto.IV, KeyDerivationPrf.HMACSHA512, 1000, KeyByteSize);

            return crypto;
        }
    }
}