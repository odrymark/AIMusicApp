using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using GeniusLyrics.NET;

namespace Api.Services.Song;

public interface ISongMetadataService
{
    Task<string> GetLyrics(string title, string artist);
    Task<int> GetBpm(Stream audioStream);
}

public partial class SongMetadataService(GeniusClient genius, IHttpClientFactory httpClientFactory) : ISongMetadataService
{
    private static string? CleanLyrics(string? raw)
    {
        if (raw == null) return null;
    
        var firstBracket = raw.IndexOf('[');
        var lyrics = firstBracket >= 0 ? raw[firstBracket..] : raw;
    
        lyrics = MyRegex().Replace(lyrics, "");
        lyrics = MyRegex1().Replace(lyrics, "\n\n").Trim();
    
        return lyrics;
    }
    
    public async Task<string> GetLyrics(string title, string artist)
    {
        var song = await genius.GetSong(title, artist);
        var cleanLyrics = CleanLyrics(song?.Lyrics);
    
        if (!string.IsNullOrWhiteSpace(cleanLyrics))
            return cleanLyrics;

        var client = httpClientFactory.CreateClient();
        var encodedTitle = Uri.EscapeDataString(title);
        var encodedArtist = Uri.EscapeDataString(artist);
        var response = await client.GetAsync($"https://api.lyrics.ovh/v1/{encodedArtist}/{encodedTitle}");
    
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadFromJsonAsync<JsonObject>();
            var lyrics = json?["lyrics"]?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(lyrics))
                return lyrics;
        }

        return "No lyrics available.";
    }

    public async Task<int> GetBpm(Stream audioStream)
    {
        var client = httpClientFactory.CreateClient("AiBackend");
    
        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(audioStream);
        fileContent.Headers.ContentType = new("audio/mpeg");
        content.Add(fileContent, "file", "audio.mp3");
    
        var response = await client.PostAsync("/bpm", content);
        response.EnsureSuccessStatusCode();
    
        var result = await response.Content.ReadFromJsonAsync<JsonObject>();
        return result!["bpm"]!.GetValue<int>();
    }

    [GeneratedRegex(@"\[.*?\]")]
    private static partial Regex MyRegex();
    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex MyRegex1();
}