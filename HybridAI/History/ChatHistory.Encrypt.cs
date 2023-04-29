using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using HybridAI.Security;

using Newtonsoft.Json;

namespace HybridAI.History
{
    internal partial class ChatHistory
    {
        private void WriteEncryptedDataAndSign()
        {
            var fileName = GetFileName();
            var signatureFileName = Path.ChangeExtension(fileName, "signature");

            using var fileStream = File.Create(Path.Combine(DirectoryName, fileName));
            using var signatureFileStream = File.Create(Path.Combine(DirectoryName, signatureFileName));

            var data = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(ChatContext));

            EncryptData(fileStream, EncryptionManager.EncryptionDescriptor, data);
            SignData(signatureFileStream, EncryptionManager.EncryptionDescriptor, data);
        }

        private static void SignData(FileStream signatureFileStream, EncryptionDescriptor encryptionDescriptor, byte[] data)
        {
            using var signatory = new HMACSHA512(encryptionDescriptor.EncryptionKey);
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
        private string GetFileName()
        {
            var title = GetTitle();
            var encodedTitle = Encoding.Unicode.GetBytes(title);
            using var authentication = new HMACSHA512(encodedTitle);
            var hash = authentication.ComputeHash(encodedTitle);

            var fileName = BitConverter.ToString(hash).Replace("-", string.Empty);
            return fileName;
        }
    }
}
