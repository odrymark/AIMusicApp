using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess;

public class Playlist
{
    public Guid id { get; set; }

    [Required]
    public Guid userId { get; set; }

    [Required]
    [MaxLength(100)]
    public string title { get; set; } = string.Empty;

    public string? image { get; set; }
    
    public bool isPublic { get; set; } = false;

    [ForeignKey(nameof(userId))]
    public User user { get; set; } = null!;

    public ICollection<Song> songs { get; set; } = new List<Song>();
}