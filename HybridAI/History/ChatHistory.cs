using System.Collections.Generic;
using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace HybridAI.History
{
    internal partial class ChatHistory
    {
        private static readonly string DirectoryName = "ChatHistory";
        public ChatHistory()
        {
        }

        public List<Message> ChatContext { get; private set; } = new();

        public void Save()
        {
            if (ChatContext.Count == 0)
            {
                return;
            }

            CheckDirectory();
            WriteDataAndSign();
        }

        public static IEnumerable<ChatHistory> Load()
        {
            CheckDirectory();

            var directoryInfo = new DirectoryInfo(DirectoryName);
            foreach (var file in directoryInfo.GetFiles())
            {
                if (file.Extension == ".signature")
                {
                    continue;
                }

                var fileName = file.Name;
                var signatureFileName = Path.Combine(DirectoryName, Path.ChangeExtension(fileName, "signature"));
                if (!File.Exists(signatureFileName))
                {
                    continue;
                }

                var credential = GetCredential();
                var decryptKey = GeKey(credential);

                var decryptedData = GetDecryptedData(file.Open(FileMode.Open), decryptKey);

                // if (!CheckSignature(decryptedData, decryptKey, File.ReadAllBytes(signatureFileName)))
                // {
                //     continue;
                // }

                yield return new ChatHistory()
                {
                    ChatContext = JsonConvert.DeserializeObject<List<Message>>(Encoding.Default.GetString(decryptedData)) ?? new List<Message>()
                };
            }
        }

        private static void CheckDirectory()
        {
            Directory.CreateDirectory(DirectoryName);
        }
    }
}
