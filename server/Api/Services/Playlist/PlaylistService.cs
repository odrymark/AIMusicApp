using Api.DTOs.Request;
using Api.DTOs.Response;
using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Playlist;

public class PlaylistService(MusicDbContext context) : IPlaylistService
{
    public async Task CreatePlaylist(Guid userId, string title, List<Guid> songIds, bool isPublic, string? imageKey = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID", nameof(userId));

        _ = await context.Users.FindAsync(userId)
            ?? throw new ArgumentException("User not found");

        var songs = await context.Songs
            .Where(s => songIds.Contains(s.id))
            .ToListAsync();

        var missingSongs = songIds.Except(songs.Select(s => s.id)).ToList();
        if (missingSongs.Count != 0)
            throw new ArgumentException($"Songs not found: {string.Join(", ", missingSongs)}");

        var playlist = new DataAccess.Playlist
        {
            userId = userId,
            title = title,
            image = imageKey,
            isPublic = isPublic,
            songs = songs
        };

        context.Playlists.Add(playlist);
        await context.SaveChangesAsync();
    }
    
    public async Task EditPlaylist(Guid userId, Guid playlistId, string title, List<Guid> songIds, bool isPublic, string? imageKey = null)
    {
        var playlist = await context.Playlists
            .Include(p => p.songs)
            .FirstOrDefaultAsync(p => p.id == playlistId);

        if (playlist == null)
            throw new KeyNotFoundException("Playlist not found");

        if (playlist.userId != userId)
            throw new UnauthorizedAccessException("You do not own this playlist");

        var songs = await context.Songs
            .Where(s => songIds.Contains(s.id))
            .ToListAsync();

        var missingSongs = songIds.Except(songs.Select(s => s.id)).ToList();
        if (missingSongs.Count != 0)
            throw new ArgumentException($"Songs not found: {string.Join(", ", missingSongs)}");

        playlist.title = title;
        playlist.isPublic = isPublic;
        playlist.songs = songs;

        if (imageKey != null)
            playlist.image = imageKey;

        await context.SaveChangesAsync();
    }
    
    public async Task<IEnumerable<PlaylistResDto>> GetUserPlaylists(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID", nameof(userId));

        var playlists = await context.Playlists
            .Where(p => p.userId == userId)
            .Include(p => p.user)
            .Include(p => p.songs)
            .OrderByDescending(p => p.id)
            .ToListAsync();

        if (playlists.Count == 0)
        {
            var userExists = await context.Users.AnyAsync(u => u.id == userId);
            if (!userExists)
                throw new ArgumentException("User not found");
        }

        return playlists.Select(p => new PlaylistResDto
        {
            id = p.id,
            title = p.title,
            creatorUser = p.user.username,
            image = p.image,
            isPublic = p.isPublic,
            songs = p.songs.Select(s => new SongResDto
            {
                id = s.id,
                title = s.title,
                songKey = s.songKey,
                artist = s.artist,
                image = s.image,
                isPublic = s.isPublic
            }).ToList()
        });
    }

    public async Task<IEnumerable<PlaylistResDto>> GetPlaylists()
    {
        var playlists = await context.Playlists
            .Where(p => p.isPublic)
            .Include(p => p.user)
            .Include(p => p.songs)
            .OrderByDescending(p => p.id)
            .ToListAsync();

        var playlistDtos = playlists.Select(p => new PlaylistResDto
        {
            id = p.id,
            title = p.title,
            creatorUser = p.user.username,
            image = p.image,
            isPublic = p.isPublic,
            songs = p.songs.Select(s => new SongResDto
            {
                id = s.id,
                title = s.title,
                songKey = s.songKey,
                artist = s.artist,
                image = s.image,
                isPublic = s.isPublic
            }).ToList()
        });

        return playlistDtos;
    }
}