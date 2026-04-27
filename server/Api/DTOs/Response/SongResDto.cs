namespace Api.DTOs.Response;

public class SongResDto
{
    public Guid id { get; set; }

    public required string title { get; set; }

    public required string songKey { get; set; }
    
    public required string artist { get; set; }

    public string? image { get; set; }
    public bool isPublic { get; set; }
}