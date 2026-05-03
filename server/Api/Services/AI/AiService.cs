using System.Text;
using System.Text.Json;
using Api.DTOs.Request;
using Api.DTOs.Response;

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

    public async Task<IEnumerable<Guid>> GetRecommendations(IEnumerable<SongResDto> listenedMoods, IEnumerable<SongResDto> songs)
    {
        var payload = new
        {
            listened_moods = listenedMoods.Select(s => s.mood).ToList(),
            available_songs = songs.Select(s => new { id = s.id, mood = s.mood }).ToList()
        };
        
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/recommend", content);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"AI backend returned {response.StatusCode}", null, response.StatusCode);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseBody);
        
        var songIds = doc.RootElement.GetProperty("song_ids")
            .EnumerateArray()
            .Select(x => Guid.Parse(x.GetString()!))
            .ToList();
    
        return songIds;
    }

}