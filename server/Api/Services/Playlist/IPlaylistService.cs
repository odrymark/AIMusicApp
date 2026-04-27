using Api.DTOs.Response;

namespace Api.Services.Playlist;

public interface IPlaylistService
{
    Task CreatePlaylist(Guid userId, string title, List<Guid> songIds, bool isPublic, string? imageKey = null);
    Task<IEnumerable<PlaylistResDto>> GetUserPlaylists(Guid userId);
    Task<IEnumerable<PlaylistResDto>> GetPlaylists();
    Task EditPlaylist(Guid userId, Guid playlistId, string title, List<Guid> songIds, bool isPublic, string? imageKey = null);
}