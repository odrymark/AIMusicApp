namespace Api.DTOs.Response;

public class SongResDto
{
    public Guid id { get; set; }

    public required string title { get; set; }

    public required string songKey { get; set; }
    
    public required string artist { get; set; }
    public required string mood { get; set; }

    public string? image { get; set; }
    public bool isPublic { get; set; }

    public static SongResDto FromSong(DataAccess.Song s) => new()
    {
        id = s.id,
        title = s.title,
        songKey = s.songKey,
        artist = s.artist,
        image = s.image,
        isPublic = s.isPublic,
        mood = s.mood
    };
}