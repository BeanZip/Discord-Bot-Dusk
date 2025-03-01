namespace Discord_Bot_Dusk;

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class OpenAIClient
{
    private readonly string apiKey;
    private readonly string endpoint = "https://api.openai.com/v1/chat/completions";
    private readonly HttpClient httpClient;

    public OpenAIClient(string? token = null)
    {
        apiKey = token ?? Environment.GetEnvironmentVariable("OpenAIKey") ?? throw new ArgumentNullException("API key is required!");
        httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public async Task<string> GetResponseAsync(string input)
    {
        var requestBody = new
        {
            model = "gpt-4",
            messages = new[] { new { role = "user", content = input } }
        };

        var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();

        string responseString = await response.Content.ReadAsStringAsync();
        return responseString;
    }
}