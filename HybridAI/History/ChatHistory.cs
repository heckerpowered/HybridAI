using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using HybridAI.Security;

using Newtonsoft.Json;

namespace HybridAI.History
{
    internal partial class ChatHistory
    {
        private static readonly string DirectoryName = "ChatHistory";
        public static IEnumerable<ChatHistory> Load()
        {
            Trace.TraceInformation("Loading chat history");
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
                    Trace.TraceError($"Unable to find signature file for {fileName}");
                    continue;
                }

                Trace.TraceInformation($"Loading chat history: {fileName}");

                var decryptedData = GetDecryptedData(file.Open(FileMode.Open), EncryptionManager.EncryptionDescriptor);
                var decryptedDataString = Encoding.Unicode.GetString(decryptedData);

                if (!CheckSignature(decryptedData, EncryptionManager.EncryptionDescriptor, File.ReadAllBytes(signatureFileName)))
                {
                    Trace.TraceError($"Unable to verify file signature");
                    continue;
                }

                yield return new ChatHistory()
                {
                    ChatContext = JsonConvert.DeserializeObject<List<Message>>(decryptedDataString) ?? new List<Message>()
                };
            }
        }

        private static void CheckDirectory()
        {
            Directory.CreateDirectory(DirectoryName);
        }

        public ChatHistory()
        {
        }

        public List<Message> ChatContext { get; private set; } = new();

        public void Save()
        {
            if (ChatContext.Count == 0)
            {
                Trace.TraceInformation("No chat history to save");
                return;
            }

            Trace.TraceInformation($"Saving chat history, title: {GetTitle()}");

            CheckDirectory();
            WriteEncryptedDataAndSign();
        }

        private string GetTitle()
        {
            return ChatContext.First().Input;
        }
    }
}
