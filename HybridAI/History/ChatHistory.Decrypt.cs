using System.IO;
using System.Security.Cryptography;

namespace HybridAI.History
{
    internal partial class ChatHistory
    {
        private static byte[] GetDecryptedData(Stream encryptedStream, byte[] decryptKey)
        {
            using var decryptor = Aes.Create();
            decryptor.Key = decryptKey;

            using var cryptoStream = new CryptoStream(encryptedStream, decryptor.CreateDecryptor(), CryptoStreamMode.Read);
            using var memoryStream = new MemoryStream();
            encryptedStream.CopyTo(memoryStream);

            return memoryStream.ToArray();
        }

        private static bool CheckSignature(byte[] decryptedData, byte[] decryptKey, byte[] storedHash)
        {
            using var signatory = new HMACSHA512(decryptKey);
            var expectedHash = signatory.ComputeHash(decryptedData);

            return expectedHash.GetHashCode() == storedHash.GetHashCode();
        }
    }
}
