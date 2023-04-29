using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace HybridAI.Update
{
    internal class UpdateChecker
    {
        private static readonly HttpClient UpdateServer = new()
        {
            BaseAddress = new("https://hybridai-1256953837.cos.ap-nanjing.myqcloud.com")
        };

        public static async Task<List<string>> CheckUpdate()
        {
            Trace.TraceInformation("Getting authentication key");
            var authenticationKey = await GetAuthenticationKey();
            var authenticationKeyString = BitConverter.ToString(authenticationKey).Replace("-", string.Empty);
            Trace.TraceInformation($"Authentication key: {authenticationKeyString}");

            Trace.TraceInformation("Writing assets json");
            WriteAssetsJson(authenticationKey);

            var localAssets = GetLocalAssets(authenticationKey);
            var latestAssets = await GetLatestAssets();
            var expiredFiles = new List<string>();

            foreach (var latestAsset in latestAssets)
            {
                var latestFileName = latestAsset.Key;
                if (!localAssets.TryGetValue(latestFileName, out var localHash)
                    || !latestAsset.Value.SequenceEqual(localHash))
                {
                    expiredFiles.Add(latestFileName);
                }
            }

            Trace.TraceInformation($"{expiredFiles.Count} files expired");
            Trace.Indent();
            foreach (var expiredFile in expiredFiles)
            {
                Trace.WriteLine(expiredFile);
            }
            Trace.Unindent();

            return expiredFiles;
        }

        public static void WriteAssetsJson(byte[] authenticationKey)
        {
            Dictionary<string, byte[]> assets = GetLocalAssets(authenticationKey);
            Trace.Indent();
            foreach (var asset in assets)
            {
                var hashString = BitConverter.ToString(asset.Value).Replace("-", string.Empty);
                Trace.WriteLine($"FileName: {asset.Key}, Hash: {hashString}");
            }
            Trace.Unindent();
            File.WriteAllText("Assets.json", JsonConvert.SerializeObject(assets, Formatting.Indented));
            Trace.TraceInformation("Assets.json successfully written");
        }

        private static Dictionary<string, byte[]> GetLocalAssets(byte[] authenticationKey)
        {
            var assets = new Dictionary<string, byte[]>();
            var directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
            var authentication = new HMACSHA512(authenticationKey);
            foreach (var fileInfo in directoryInfo.EnumerateFiles())
            {
                using var fileStream = fileInfo.OpenRead();
                var fileName = fileInfo.Name;
                var hash = authentication.ComputeHash(fileStream);

                assets.Add(fileName, hash);
            }

            return assets;
        }

        public static async Task<byte[]> GetAuthenticationKey() => await UpdateServer.GetByteArrayAsync("AuthenticationKey.txt");

        public static async Task<Dictionary<string, byte[]>> GetLatestAssets()
        {
            var assetsString = await UpdateServer.GetStringAsync("Assets.json");
            var assets = JsonConvert.DeserializeObject<Dictionary<string, byte[]>>(assetsString);

            return assets ?? throw new NullReferenceException();
        }

        public static async void Update(List<string> expiredFiles, ProgressCallback callback)
        {
            File.Delete("Assets.json");
            Trace.TraceInformation("Updating");
            Directory.CreateDirectory("UpdateFiles");

            var locker = new object();
            var totalContentLength = 0;
            var downloadProgress = 0.0;
            var downloadedBytes = 0;
            var tasks = new List<Task<(string ExpiredFileName, Stream NetworkStream)>>();


            foreach (var expiredFile in expiredFiles)
            {
                var task = Task.Run(() =>
                {
                    var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Path.Combine("Binary", expiredFile));
                    var response = UpdateServer.Send(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    var contentLength = int.Parse(response.Content.Headers.GetValues("Content-Length").First());

                    lock (locker)
                    {
                        totalContentLength += contentLength;
                    }

                    return (expiredFile, response.Content.ReadAsStream());
                });
                tasks.Add(task);
            }

            var downloadTasks = new List<Task>();
            foreach (var task in tasks)
            {
                var updateInformation = await task;
                var localFileName = Path.Combine("UpdateFiles", updateInformation.ExpiredFileName);
                using var localFileStream = File.Create(localFileName);
                using var networkStream = updateInformation.NetworkStream;

                Trace.TraceInformation($"Downloading file: {updateInformation.ExpiredFileName}");

                await Task.Run(() =>
                {
                    while (true)
                    {
                        var buffer = new byte[8192];
                        var bytesRead = networkStream.Read(buffer);
                        if (bytesRead == 0)
                        {
                            break;
                        }

                        localFileStream.Write(buffer, 0, bytesRead);
                        lock (locker)
                        {
                            downloadedBytes += bytesRead;
                            downloadProgress = ((double)downloadedBytes / totalContentLength) * 100;
                            callback(downloadProgress);
                        }
                    }
                });
            }
            Trace.TraceInformation("Download complete");

            foreach (var expiredFile in expiredFiles)
            {
                Trace.TraceInformation($"Moving file {expiredFile}");
                File.Move(expiredFile, $"{expiredFile}.expired");
                File.Move(Path.Combine("UpdateFiles", expiredFile), expiredFile);
            }
        }

        public delegate void ProgressCallback(double Value);
    }
}
