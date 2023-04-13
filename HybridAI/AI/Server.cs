using System;
using System.IO;
using System.Net.Http;

using Newtonsoft.Json;

namespace HybridAI.AI
{
    internal class Server
    {
        private static HttpClient Client { get; } = new();

        public static async void RequestAIStream(MessageRequest request, DiscontinuousMessageReceiver discontinuousMessageReceiver, ExceptionHandler exceptionHandler)
        {
            var serializedRequest = JsonConvert.SerializeObject(request);
            var content = new StringContent(serializedRequest);
            var response = await Client.PostAsync("http://47.104.91.156:8880/sse/subscribe", content);

            try
            {
                // Exceptions raised by asynchronous methods cannot be
                // caught by previous call via try/catch blocks
                response.EnsureSuccessStatusCode();
            }
            catch (Exception exception)
            {
                exceptionHandler(exception);
                return;
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(stream);

            var buffer = new char[8192];

            var numberOfCharactersRead = await streamReader.ReadBlockAsync(buffer);
            if (numberOfCharactersRead == 0)
            {
                discontinuousMessageReceiver(string.Empty);
                return;
            }

            discontinuousMessageReceiver(new string(buffer, 0, numberOfCharactersRead));
        }
    }
}
