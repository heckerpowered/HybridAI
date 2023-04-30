using System.Net.Http;

namespace HybridAI.Network
{
    internal static class NetworkManager
    {
        public static HttpClientHandler HttpClientHandler { get; } = new();
    }
}
