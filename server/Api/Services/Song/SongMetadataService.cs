using System.Text.RegularExpressions;
using GeniusLyrics.NET;

namespace Api.Services.Song;

public partial class SongMetadataService(GeniusClient genius) : ISongMetadataService
{
    private static string? CleanLyrics(string? raw)
    {
        if (raw == null) return null;

        var firstBracket = raw.IndexOf('[');
        var lyrics = firstBracket >= 0 ? raw[firstBracket..] : raw;

        lyrics = BracketTagRegex().Replace(lyrics, "");

        lyrics = ExcessiveNewlinesRegex().Replace(lyrics, "\n\n").Trim();

        return lyrics;
    }

    public async Task<string> GetMetadataAsync(string title, string artist)
    {
        var song = await genius.GetSong(title, artist);

        var cleanLyrics = CleanLyrics(song?.Lyrics);

        return cleanLyrics ?? string.Empty;
    }

    [GeneratedRegex(@"\[.*?\]")]
    private static partial Regex BracketTagRegex();

    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex ExcessiveNewlinesRegex();
}
