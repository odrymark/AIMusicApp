using System.Text.RegularExpressions;
using GeniusLyrics.NET;

namespace Api.Services.Song;

public interface ISongMetadataService
{
    Task<string> GetMetadataAsync(string title, string artist);
}

public partial class SongMetadataService(GeniusClient genius) : ISongMetadataService
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
    
    public async Task<string> GetMetadataAsync(string title, string artist)
    {
        var song = await genius.GetSong(title, artist);
        
        var cleanLyrics = CleanLyrics(song?.Lyrics);

        return cleanLyrics ?? string.Empty;
    }

    [GeneratedRegex(@"\[.*?\]")]
    private static partial Regex MyRegex();
    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex MyRegex1();
}