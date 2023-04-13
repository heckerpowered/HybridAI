using Newtonsoft.Json;

namespace HybridAI.AI
{
    internal class MessageRequest
    {
        public MessageRequest(string message, string id)
        {
            Message = message;
            Id = id;
        }

        [JsonProperty("message")]
        public string Message { get; }

        [JsonProperty("id")]
        public string Id { get; }
    }
}
