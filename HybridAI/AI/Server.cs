using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace HybridAI.AI
{
    internal class Server
    {
        internal static HttpClient Client { get; } = new();

        public static async Task RequestAIStream(MessageRequest request, DiscontinuousMessageReceiver discontinuousMessageReceiver, ExceptionHandler exceptionHandler, CancellationToken cancellationToken)
        {
            var serializedRequest = JsonConvert.SerializeObject(request);
            var content = new StringContent(serializedRequest, new MediaTypeHeaderValue("application/json", "utf-8"));

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "http://47.104.91.156:8080/gpt/subscribe/text")
            {
                Content = content
            };

            Trace.TraceInformation($"Begin AI request, id: {request.Id}");
            var performanceCounter = Stopwatch.StartNew();

            try
            {
                var response = await Client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                Trace.TraceInformation($"Response headers read, elapsed time: {performanceCounter.Elapsed}");

                // Exceptions raised by asynchronous methods cannot be
                // caught by previous call via try/catch blocks
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var streamReader = new StreamReader(stream);

                while (true)
                {
                    var buffer = new char[8192];
                    var numberOfCharactersRead = await streamReader.ReadAsync(buffer, cancellationToken);
                    if (numberOfCharactersRead == 0)
                    {
                        break;
                    }

                    var message = new string(buffer, 0, numberOfCharactersRead);
                    foreach (var splitMessage in message.Split("\n\n"))
                    {
                        var plainText = splitMessage.Replace("data:", string.Empty);
                        if (string.IsNullOrEmpty(plainText))
                        {
                            continue;
                        }

                        await discontinuousMessageReceiver(plainText);
                    }
                }

                await discontinuousMessageReceiver(string.Empty);
                Trace.TraceInformation($"AI request end, elapsed time: {performanceCounter.Elapsed}");
            }
            catch (TaskCanceledException)
            {
                Trace.TraceInformation($"AI request canceled, elapsed time: {performanceCounter.Elapsed}");
            }
            catch (Exception exception)
            {
                exceptionHandler(exception);
            }
        }
    }
}
