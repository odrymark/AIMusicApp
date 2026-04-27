using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Request;

public class PlaylistCreateReqDto
{
    [Required]
    [MinLength(3)]
    [MaxLength(100)]
    public required string title { get; set; }
    
    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public required List<Guid> songIds { get; set; }
    
    public IFormFile? image { get; set; }
    
    [Required]
    public required bool isPublic { get; set; }
}