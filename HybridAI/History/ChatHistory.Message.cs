namespace HybridAI.History
{
    internal class Message
    {
        public Message(string input, string response)
        {
            Input = input;
            Response = response;
        }

        public string Input { get; }
        public string Response { get; }
    }
}
