using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace HybridAI.AI
{
    internal class Server
    {
        private static HttpClient Client { get; } = new();
        public static async Task<string> RequestAI(MessageRequest request)
        {
            var serializedRequest = JsonConvert.SerializeObject(request);

            var response = await Client.PostAsync("http://47.104.91.156:8880/demo", new StringContent(serializedRequest));
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
