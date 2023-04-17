using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using Newtonsoft.Json;

namespace HybridAI.History
{
    internal partial class ChatHistory
    {
        private void WriteEncryptedDataAndSign()
        {
            var credential = GetCredential();
            using var credentialWithContext = GetCredentialWithContext(credential);

            var dataFileName = BitConverter.ToString(SHA512.HashData(credentialWithContext)).Replace("-", string.Empty);
            var signatureFileName = Path.ChangeExtension(dataFileName, "signature");

            using var dataFileStream = File.Create(Path.Combine(DirectoryName, dataFileName));
            using var signatureFileStream = File.Create(Path.Combine(DirectoryName, signatureFileName));

            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ChatContext));

            EncryptData(dataFileStream, EncryptionDescriptor, data);
            SignData(signatureFileStream, EncryptionDescriptor.EncryptionKey, data);
        }

        private static void SignData(FileStream signatureFileStream, byte[] encryptKey, byte[] data)
        {
            using var signatory = new HMACSHA512(encryptKey);
            signatureFileStream.Write(signatory.ComputeHash(data));
        }

        private static void EncryptData(Stream outputStream, EncryptionDescriptor encryptionDescriptor, byte[] data)
        {
            using var encryptor = Aes.Create();
            encryptor.Key = encryptionDescriptor.EncryptionKey;
            encryptor.IV = encryptionDescriptor.InitializationVector;

            using var cryptoStream = new CryptoStream(outputStream, encryptor.CreateEncryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(data);
        }
    }
}
