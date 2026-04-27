namespace Api.DTOs.Response;

public class PlaylistResDto
{
    public Guid id { get; set; }

    public required string title { get; set; }
    
    public required List<SongResDto> songs { get; set; }
    
    public required string creatorUser { get; set; }

    public string? image { get; set; }
    
    public bool isPublic { get; set; }
}