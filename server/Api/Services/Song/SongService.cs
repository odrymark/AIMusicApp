using Api.DTOs.Response;
using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Song;

public class SongService(MusicDbContext context) : ISongService
{
    public async Task CreateSong(Guid userId, string title, string songKey,  string artist, bool isPublic, string mood, string? imageKey = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));
        if (string.IsNullOrWhiteSpace(songKey))
            throw new ArgumentException("songKey cannot be empty", nameof(songKey));

        var song = new DataAccess.Song
        {
            id = Guid.NewGuid(),
            userId = userId,
            title = title,
            songKey = songKey,
            artist = artist,
            isPublic = isPublic,
            image = imageKey,
            mood = mood
        };

        context.Songs.Add(song);
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<SongResDto>> GetUserSongs(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID", nameof(userId));

        var songs = await context.Songs
            .Where(s => s.userId == userId)
            .OrderByDescending(s => s.id)
            .ToListAsync();

        var songDtos = songs.Select(s => new SongResDto
        {
            id = s.id,
            title = s.title,
            songKey = s.songKey,
            artist =  s.artist,
            image = s.image,
            isPublic = s.isPublic,
            mood = s.mood
        });

        return songDtos;
    }

    public async Task<IEnumerable<SongResDto>> GetSongs()
    {
        var songs = await context.Songs
            .Where(s => s.isPublic)
            .OrderByDescending(s => s.id)
            .ToListAsync();

        var songDtos = songs.Select(s => new SongResDto
        {
            id = s.id,
            title = s.title,
            songKey = s.songKey,
            artist = s.artist,
            image = s.image,
            isPublic = s.isPublic,
            mood = s.mood
        });
        
        return songDtos;
    }
    
    public async Task<IEnumerable<SongResDto>> GetSongsById(IEnumerable<Guid> songIds)
    {
        var songs = await context.Songs
            .Where(s => songIds.Contains(s.id))
            .ToListAsync();

        return songs.Select(s => new SongResDto
        {
            id = s.id,
            title = s.title,
            songKey = s.songKey,
            artist = s.artist,
            image = s.image,
            isPublic = s.isPublic,
            mood = s.mood
        });
    }

    public async Task<IEnumerable<SongResDto>> GetRecentSongs(Guid userId)
    {
        var songs = await context.History
            .Where(h => h.userId == userId)
            .Include(h => h.song)
            .OrderByDescending(h => h.playedAt)
            .ToListAsync();
        
        return songs.Select(h => new SongResDto
        {
            id = h.id,
            title = h.song.title,
            songKey = h.song.songKey,
            artist = h.song.artist,
            image = h.song.image,
            isPublic = h.song.isPublic,
            mood = h.song.mood
        });
    }
    
    public async Task<IEnumerable<SongResDto>> GetRandomSongs(int count = 10)
    {
        var songs = await context.Songs
            .OrderBy(_ => EF.Functions.Random())
            .Take(count)
            .ToListAsync();

        return songs.Select(s => new SongResDto
        {
            id = s.id,
            title = s.title,
            songKey = s.songKey,
            artist = s.artist,
            image = s.image,
            isPublic = s.isPublic,
            mood = s.mood
        });
    }

    public async Task AddHistory(Guid userId, Guid songId)
    {
        var history = new History
        {
            userId = userId,
            songId = songId,
            playedAt = DateTime.UtcNow
        };
        context.History.Add(history);
        await context.SaveChangesAsync();
    }

    public async Task EditSong(Guid userId, Guid songId, string title, string artist, bool isPublic, string? imageKey = null)
    {
        var song = await context.Songs.FirstOrDefaultAsync(s => s.id == songId);

        if (song == null)
            throw new KeyNotFoundException("Song not found");

        if (song.userId != userId)
            throw new UnauthorizedAccessException("You do not own this song");

        song.title = title;
        song.artist = artist;
        song.isPublic = isPublic;

        if (imageKey != null)
            song.image = imageKey;

        await context.SaveChangesAsync();
    }
}