using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace StreamArtist
{

    public class NetworkService
    {
        private readonly HttpClient _client;

        public NetworkService()
        {
            var handler = new HttpClientHandler();
            _client = new HttpClient(handler);
        }

        public async Task<Dictionary<string, string>> PostAsync(string server, string action, Dictionary<string, string> settings)
        {
            try
            {
                var requestDict = new Dictionary<string, string>(settings)
                {
                    ["action"] = action
                };
                string jsonString = JsonSerializer.Serialize(requestDict);
                StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                Console.WriteLine("Posting " + server + " with " + jsonString);
                HttpResponseMessage response = await _client.PostAsync(server, content);
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
                result["status"] = "ok";
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new Dictionary<string, string>
                {
                    ["control-server-status"] = "Error connecting to control server."
                };
            }
        }
    }
}