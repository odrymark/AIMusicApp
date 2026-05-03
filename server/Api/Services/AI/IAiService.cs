using Api.DTOs.Request;
using Api.DTOs.Response;

namespace Api.Services.AI;

public interface IAiService
{
    Task<string> GetSongMood(string lyrics, int bpm);
    Task<IEnumerable<Guid>> GetRecommendations(IEnumerable<SongResDto> listenedMoods, IEnumerable<SongResDto> songs);
}