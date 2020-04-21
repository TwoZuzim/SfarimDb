using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace Sfarim.Proxies
{
    public interface ISeferProxy
    {
        Task<ApiSefer> GetGemara(string seder, string name);
    }

    public class SeferProxy : ISeferProxy
    {
        public IWebHostEnvironment Env { get; }
        public HttpClient Client { get; }
        public SeferProxy(IHttpClientFactory clientFactory, IWebHostEnvironment env)
        {
            Env = env;
            Client = clientFactory.CreateClient();
        }

        public async Task<ApiSefer> GetGemara(string seder, string name)
        {
            var directoryPath = Path.Combine(Env.WebRootPath, "docs_cache");
            var filePath = Path.Combine(directoryPath, $"{name}.json");
            if (!File.Exists(filePath))
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                var apiResult = await Client.GetStringAsync($"https://raw.githubusercontent.com/Sefaria/Sefaria-Export/master/json/Talmud/Bavli/Seder%20{seder}/{name}/Hebrew/Wikisource%20Talmud%20Bavli.json").ConfigureAwait(false);
                File.WriteAllText(filePath, apiResult);
            }

            var jsonString = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<ApiSefer>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}
