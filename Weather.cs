using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;

public class Weather{
    public async Task<string> GetWeatherApi(string city)
    {
        string? apiKey = Environment.GetEnvironmentVariable("WKey");
        string url = $"https://api.weatherapi.com/v1/current.json?key={apiKey}&q={city}";
        
        Console.WriteLine($"{apiKey} is valid" );
        using HttpClient client = new HttpClient();
        try
        {
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(responseBody);

            string? tempC = json["current"]?["temp_c"]?.ToString();
            string? condition = json["current"]?["condition"]?["text"]?.ToString();
            
            return $"Weather in {city}:\nTemperature: {tempC}°C\nCondition: {condition}";
        }
        catch (HttpRequestException e)
        {
            return $"Error fetching weather data: {e.Message}";
        }
    }
}