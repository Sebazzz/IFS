// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : CryptoMetadata.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core.Crypto {
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    internal static class CryptoMetadata {
        public static void WriteMetadata(Stream outputStream, Aes algorithm) {
            using (BinaryWriter bw = new BinaryWriter(outputStream, Encoding.UTF8, true)) {
                bw.Write(algorithm.IV.Length);
                bw.Write(algorithm.IV);
            }
        }

        public static Aes ReadMetadataAndInitializeAlgorithm(Stream inputStream, string password) {
            byte[] iv;

            using (BinaryReader br = new BinaryReader(inputStream, Encoding.UTF8, true)) {
                int ivLength = br.ReadInt32();
                iv = new byte[ivLength];

                int readBytes = br.Read(iv, 0, iv.Length);
                if (readBytes != iv.Length) {
                    throw new InvalidDataException($"Unable to read IV: expected to read {iv.Length} bytes, but got {readBytes}");
                }
            }

            return CryptoFactory.CreateCrypto(password, iv);
        }
    }
}