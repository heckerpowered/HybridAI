﻿using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HybridAI.History
{
    internal partial class ChatHistory
    {
        private const int KeySize = 32;
        private const int InitializationVectorSize = 16;

        internal static EncryptionDescriptor EncryptionDescriptor { get; } = EncryptionDescriptor.GetEncryptionDescriptor();

        /// <summary>
        /// Generate a credential to encrypt the chat history, the credential is a SHA512 hash of the machine name with UTF8 encoding.
        /// </summary>
        /// <returns>Credential to encrypt the chat history, this credential may need to be transformed into a key for the encryption algorithm.</returns>
        internal static byte[] GetCredential()
        {
            return SHA512.HashData(Encoding.UTF8.GetBytes(Environment.MachineName));
        }

        /// <summary>
        /// Assembling the provided credentials with the title of the chat history and using SHA512
        /// to calculate the combined data yields the file name of the chat history.
        /// </summary>
        /// <param name="credential">Credential generated by <c>GetCredential()</c> method</param>
        /// <returns>A memory stream containing credential and the title of the chat history, the position of the memory stream
        /// will be set to 0 before returning.</returns>
        private MemoryStream GetCredentialWithContext(byte[] credential)
        {
            var memoryStream = new MemoryStream();

            memoryStream.Write(credential);
            memoryStream.Write(Encoding.Default.GetBytes(ChatContext.First().Input));

            memoryStream.Position = 0;
            return memoryStream;
        }

        /// <summary>
        /// Get the encryption key for AES-256 encryption
        /// </summary>
        /// <param name="credential">Credential generated by <c>GetCredential()</c> method</param>
        /// <returns>Encryption key for AES-256 encryption</returns>
        internal static byte[] GetEncryptionKey(byte[] credential)
        {
            return GetFilledOrTrimmedByteArray(credential, KeySize);
        }

        /// <summary>
        /// Get the initialization vector for AES-256 encryption
        /// </summary>
        /// <param name="credential">Credential generated by <c>GetCredential()</c> method</param>
        /// <returns>Initialization vector for AES-256 encryption</returns>
        internal static byte[] GetInitializationVector(byte[] credential)
        {
            return GetFilledOrTrimmedByteArray(credential, InitializationVectorSize);
        }

        /// <summary>
        /// Returns a byte array of the specified size by either filling the input byte array with itself or trimming it.
        /// </summary>
        /// <param name="input">The input byte array to be filled or trimmed.</param>
        /// <param name="size">The size of the output byte array.</param>
        /// <returns>A byte array of the specified size.</returns>
        private static byte[] GetFilledOrTrimmedByteArray(byte[] credential, int size)
        {
            if (credential.Length == size)
            {
                return credential;
            }
            else if (credential.Length < size)
            {
                using var memoryStream = new MemoryStream(size);

                var times = size / credential.Length + 1;
                for (int i = 0; i < times; ++i)
                {
                    memoryStream.Write(credential);
                }

                memoryStream.SetLength(size);
                return memoryStream.ToArray();
            }
            else
            {
                return credential.Take(size).ToArray();
            }
        }

        /// <summary>
        /// Get encryption algorithm for chat history encryption and decryption
        /// </summary>
        /// <returns>Symmetric encryption algorithm</returns>
        private static SymmetricAlgorithm GetEncryptionAlgorithm()
        {
            return Aes.Create();
        }
    }
}
