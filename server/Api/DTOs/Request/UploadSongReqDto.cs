using Microsoft.AspNetCore.Mvc;

namespace Api.DTOs.Request;

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class UploadSongReqDto
{
    [Required]
    public IFormFile file { get; set; } = null!;

    [Required]
    [MinLength(3)]
    [MaxLength(100)]
    public required string title { get; set; } = string.Empty;
    
    [Required]
    [MinLength(3)]
    [MaxLength(100)]
    public required string artist { get; set; } = string.Empty;
    
    [Required]
    public required bool isPublic { get; set; }
    
    public IFormFile? image { get; set; }
}