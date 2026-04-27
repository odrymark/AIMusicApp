using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccess;

public class User
{
    public Guid id { get; set; }

    [Required]
    [MaxLength(50)]
    public string username { get; set; } = string.Empty;

    [Required]
    public string password { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string email { get; set; } = string.Empty;
    
    [Required]
    public bool isAdmin { get; set; } = false;
    
    public string? refreshToken { get; set; }
    public DateTime? refreshTokenExpiry { get; set; }

    public ICollection<Song> songs { get; set; } = new List<Song>();
    public ICollection<Playlist> playlists { get; set; } = new List<Playlist>();
}