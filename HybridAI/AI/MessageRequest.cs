using Newtonsoft.Json;

namespace HybridAI.AI
{
    internal class MessageRequest
    {
        public MessageRequest(string message)
        {
            Message = message;
        }

        [JsonProperty("message")]
        public string Message { get; }
    }
}
