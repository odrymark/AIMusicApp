using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class MusicDbContext : DbContext
{
    public MusicDbContext(DbContextOptions<MusicDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Song>()
            .HasOne(s => s.user)
            .WithMany(u => u.songs)
            .HasForeignKey(s => s.userId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Playlist>()
            .HasOne(p => p.user)
            .WithMany(u => u.playlists)
            .HasForeignKey(p => p.userId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Playlist>()
            .HasMany(p => p.songs)
            .WithMany(s => s.playlists)
            .UsingEntity<Dictionary<string, object>>(
                "PlaylistSongs",
                j => j
                    .HasOne<Song>()
                    .WithMany()
                    .HasForeignKey("SongId")
                    .OnDelete(DeleteBehavior.Restrict),
                j => j
                    .HasOne<Playlist>()
                    .WithMany()
                    .HasForeignKey("PlaylistId")
                    .OnDelete(DeleteBehavior.Restrict)
            );
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Song> Songs { get; set; }
    public DbSet<Playlist> Playlists { get; set; }
}