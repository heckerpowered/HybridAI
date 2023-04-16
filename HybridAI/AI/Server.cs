﻿using System;
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

        public static async void RequestAIStream(MessageRequest request, DiscontinuousMessageReceiver discontinuousMessageReceiver, ExceptionHandler exceptionHandler, CancellationToken cancellationToken)
        {
            var serializedRequest = JsonConvert.SerializeObject(request);
            var content = new StringContent(serializedRequest, new MediaTypeHeaderValue("application/json", "utf-8"));

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "http://47.104.91.156:8080/gpt/subscribe/text")
            {
                Content = content
            };

            try
            {
                var response = await Client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                // Exceptions raised by asynchronous methods cannot be
                // caught by previous call via try/catch blocks
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                using var streamReader = new StreamReader(stream);

                while (true)
                {
                    var buffer = new char[8192];
                    var numberOfCharactersRead = await streamReader.ReadAsync(buffer, cancellationToken);
                    if (numberOfCharactersRead == 0)
                    {
                        break;
                    }

                    await discontinuousMessageReceiver(new string(buffer, 0, numberOfCharactersRead));
                }

                await discontinuousMessageReceiver(string.Empty);
            }
            catch (TaskCanceledException)
            {
                ;
            }
            catch (Exception exception)
            {
                exceptionHandler(exception);
                return;
            }
        }
    }
}
