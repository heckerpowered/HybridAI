using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using Newtonsoft.Json;

namespace HybridAI.History
{
    internal partial class ChatHistory
    {
        private void WriteDataAndSign()
        {
            var credential = GetCredential();
            using var credentialWithContext = GetCredentialWithContext(credential);

            var dataFileName = BitConverter.ToString(SHA512.HashData(credentialWithContext)).Replace("-", string.Empty);
            var signatureFileName = Path.ChangeExtension(dataFileName, "signature");

            using var dataFileStream = File.Create(Path.Combine(DirectoryName, dataFileName));
            using var signatureFileStream = File.Create(Path.Combine(DirectoryName, signatureFileName));

            var encryptKey = GeKey(credential);
            var dataToEncrypt = Encoding.Default.GetBytes(JsonConvert.SerializeObject(ChatContext));

            WriteEncryptedData(dataFileStream, encryptKey, dataToEncrypt);
            WriteSignature(signatureFileStream, encryptKey, dataToEncrypt);
        }

        private static void WriteSignature(Stream signatureFileStream, byte[] encryptKey, byte[] dataToEncrypt)
        {
            using var signatory = new HMACSHA512(encryptKey);
            signatureFileStream.Write(signatory.ComputeHash(dataToEncrypt));
        }

        private static void WriteEncryptedData(Stream dataStream, byte[] encryptKey, byte[] dataToEncrypt)
        {
            using var encryptor = Aes.Create();
            encryptor.Key = encryptKey;
            // using var cryptoStream = new CryptoStream(dataStream, encryptor.CreateEncryptor(), CryptoStreamMode.Write);
            dataStream.Write(dataToEncrypt);
        }
    }
}
