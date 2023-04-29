using System.IO;
using System.Linq;
using System.Security.Cryptography;

using HybridAI.Security;

namespace HybridAI.History
{
    internal partial class ChatHistory
    {
        private static byte[] GetDecryptedData(Stream encryptedDataStream, EncryptionDescriptor encryptionDescriptor)
        {
            using var encryptionAlgorithm = EncryptionManager.GetEncryptionAlgorithm();
            encryptionAlgorithm.Key = encryptionDescriptor.EncryptionKey;
            encryptionAlgorithm.IV = encryptionDescriptor.InitializationVector;

            using var cryptoStream = new CryptoStream(encryptedDataStream, encryptionAlgorithm.CreateDecryptor(), CryptoStreamMode.Read);
            using var memoryStream = new MemoryStream();
            cryptoStream.CopyTo(memoryStream);

            return memoryStream.ToArray();
        }

        private static bool CheckSignature(byte[] decryptedData, EncryptionDescriptor encryptionDescriptor, byte[] storedHash)
        {
            using var signatory = new HMACSHA512(encryptionDescriptor.EncryptionKey);
            var expectedHash = signatory.ComputeHash(decryptedData);

            return expectedHash.SequenceEqual(storedHash);
        }
    }
}
