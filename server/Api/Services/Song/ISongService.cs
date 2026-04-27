using Api.DTOs.Response;

namespace Api.Services.Song;

public interface ISongService
{
    Task CreateSong(Guid userId, string title, string songKey, string artist, bool isPublic, string? imageKey = null);
    Task<IEnumerable<SongResDto>> GetUserSongs(Guid userId);
    Task<IEnumerable<SongResDto>> GetSongs();
    Task EditSong(Guid userId, Guid songId, string title, string artist, bool isPublic, string? imageKey = null);
}