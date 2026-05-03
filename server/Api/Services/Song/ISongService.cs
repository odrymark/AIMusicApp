using Api.DTOs.Response;

namespace Api.Services.Song;

public interface ISongService
{
    Task CreateSong(Guid userId, string title, string songKey, string artist, bool isPublic, string mood, string? imageKey = null);
    Task<IEnumerable<SongResDto>> GetUserSongs(Guid userId);
    Task<IEnumerable<SongResDto>> GetSongs();
    Task<IEnumerable<SongResDto>> GetSongsById(IEnumerable<Guid> songIds);
    Task<IEnumerable<SongResDto>> GetRecentSongs(Guid userId);
    Task AddHistory(Guid userId, Guid songId);
    Task EditSong(Guid userId, Guid songId, string title, string artist, bool isPublic, string? imageKey = null);
    Task<IEnumerable<SongResDto>> GetRandomSongs(int count = 10);
}