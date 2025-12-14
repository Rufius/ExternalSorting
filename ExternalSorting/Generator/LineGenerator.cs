using System.Text.Json.Nodes;

namespace ExternalSorting.Generator {
    public class LineGenerator : ILineGenerator {
        public async Task<string?> GenerateAsync() {
            string? responseContent = await GetApiResponseContentAsync();
            if (responseContent == null) {
                throw new Exception("Error fetching data from remote API");
            }

            var jArray = JsonArray.Parse(responseContent);
            return jArray?[0]?["quote"]?.GetValue<string>();
        }

        public async Task<string?> GetApiResponseContentAsync() {
            using (var client = new HttpClient()) {
                using (var request = new HttpRequestMessage(HttpMethod.Get, 
                    "https://api.api-ninjas.com/v2/randomquotes")) {
                    request.Headers.Add("X-Api-Key", "D3HlLNQ54c/RWABrgOGdbQ==7f43dGj8JMQJjrqP");
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode) {
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }

            return null;
        }
    }
}
