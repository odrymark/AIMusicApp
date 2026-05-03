namespace DataAccess;

public class History
{
    public Guid id { get; set; }
    public Guid userId { get; set; }
    public Guid songId { get; set; }
    public DateTime playedAt { get; set; }
    
    public User user { get; set; } = null!;
    public Song song { get; set; } = null!;
}