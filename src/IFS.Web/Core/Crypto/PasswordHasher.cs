// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : PasswordHasher.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core.Crypto {
    using System.Text;
    using Microsoft.AspNetCore.Cryptography.KeyDerivation;

    internal static class PasswordHasher {
        public static string HashPassword(string password, string securityToken) {
            byte[] salt = Encoding.ASCII.GetBytes(securityToken);
            byte[] hash = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA512, 10000, 256 / 8);

            StringBuilder sb = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash) {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }
    }
}
