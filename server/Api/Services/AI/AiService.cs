using System.Text;
using System.Text.Json;
using Api.DTOs.Request;

namespace Api.Services.AI;

public class AiService(IHttpClientFactory httpClientFactory) : IAiService
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("AiBackend");

    public async Task<string> GetSongMood(string lyrics, int bpm)
    {
        var payload = new { lyrics, bpm };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/classify", content);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"AI backend returned {response.StatusCode}", null, response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseBody);
        return doc.RootElement.GetProperty("mood").GetString()!;
    }
}