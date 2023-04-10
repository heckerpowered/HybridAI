using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HybridAI.History
{
    internal partial class ChatHistory
    {
        private const int KeySize = 32;

        private static byte[] GetCredential()
        {
            return SHA512.HashData(Encoding.UTF8.GetBytes(Environment.MachineName));
        }

        private MemoryStream GetCredentialWithContext(byte[] credential)
        {
            var memoryStream = new MemoryStream();

            memoryStream.Write(credential);
            memoryStream.Write(Encoding.Default.GetBytes(ChatContext.First().Input));

            memoryStream.Position = 0;
            return memoryStream;
        }

        private static byte[] GeKey(byte[] credential)
        {
            if (credential.Length == KeySize)
            {
                return credential;
            }
            else if (credential.Length < KeySize)
            {
                using var memoryStream = new MemoryStream(KeySize);

                var times = KeySize / credential.Length + 1;
                for (int i = 0; i < times; ++i)
                {
                    memoryStream.Write(credential);
                }

                memoryStream.SetLength(KeySize);
                return memoryStream.ToArray();
            }
            else
            {
                return credential.Take(KeySize).ToArray();
            }
        }
    }
}
