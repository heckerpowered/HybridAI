using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace HybridAI.Security
{
    internal record EncryptionDescriptor(byte[] EncryptionKey, byte[] InitializationVector)
    {
        public static EncryptionDescriptor GetEncryptionDescriptor()
        {
            var credential = EncryptionManager.GetCredential();
            return GetEncryptionDescriptor(credential);
        }

        public static EncryptionDescriptor GetEncryptionDescriptor(byte[] credential)
        {
            var encryptionKey = EncryptionManager.GetEncryptionKey(credential);
            var initializationVector = EncryptionManager.GetInitializationVector(credential);

            return new(encryptionKey, initializationVector);
        }

        public CryptoStream GetCryptoStream(Stream stream, CryptoStreamMode cryptoStreamMode)
        {
            using var encryptionAlgorithm = EncryptionManager.GetEncryptionAlgorithm();
            encryptionAlgorithm.Key = EncryptionKey;
            encryptionAlgorithm.IV = InitializationVector;

            var transform = cryptoStreamMode == CryptoStreamMode.Read ? encryptionAlgorithm.CreateDecryptor() : encryptionAlgorithm.CreateEncryptor();
            using var cryptoStream = new CryptoStream(stream, transform, cryptoStreamMode);

            return cryptoStream;
        }

        public virtual bool Equals(EncryptionDescriptor? encryptionDescriptor)
        {
            if (encryptionDescriptor == null)
            {
                return false;
            }

            return encryptionDescriptor.EncryptionKey.SequenceEqual(EncryptionKey)
                && encryptionDescriptor.InitializationVector.SequenceEqual(InitializationVector);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(EncryptionKey.GetHashCode(), InitializationVector.GetHashCode());
        }
    }
}
