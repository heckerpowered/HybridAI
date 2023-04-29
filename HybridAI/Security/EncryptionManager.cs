using System;
using System.Security.Cryptography;
using System.Text;

using Konscious.Security.Cryptography;

namespace HybridAI.Security
{
    internal static class EncryptionManager
    {
        public static EncryptionDescriptor EncryptionDescriptor { get; set; } = EncryptionDescriptor.GetEncryptionDescriptor();
        public static byte[] GetEncryptionKey(byte[] credential, int encryptionKeySize = 32)
        {
            using var authentication = new HMACSHA512(credential);
            var salt = authentication.ComputeHash(credential);

            var argon = new Argon2id(credential)
            {
                Salt = salt,
                Iterations = 128,
                DegreeOfParallelism = Environment.ProcessorCount,
                MemorySize = 8192
            };

            return argon.GetBytes(encryptionKeySize);
        }

        public static byte[] GetInitializationVector(byte[] credential, int initializationVectorSize = 16)
            => GetEncryptionKey(credential, initializationVectorSize);

        public static Aes GetEncryptionAlgorithm() => Aes.Create();

        /// <summary>
        /// Generate a credential to encrypt the chat history, the credential is a HMACSHA512 hash of the user name with UTF8 encoding.
        /// </summary>
        /// <returns>Credential to encrypt the chat history, this credential may need to be transformed into a key for the encryption algorithm.</returns>
        public static byte[] GetCredential()
        {
            var credential = Encoding.Unicode.GetBytes(Environment.UserName);
            using var authentication = new HMACSHA512(credential);

            return authentication.ComputeHash(credential);
        }
    }
}
