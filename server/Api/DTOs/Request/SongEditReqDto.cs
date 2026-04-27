using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Request;

public class SongEditReqDto
{
    [Required]
    public required Guid id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public required string title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public required string artist { get; set; } = string.Empty;
    
    [Required]
    public required bool isPublic { get; set; }
    
    public IFormFile? image { get; set; }
    public string? prevImgKey { get; set; }
}