using Api.DTOs.Request;

namespace Api.Services.AI;

public interface IAiService
{
    Task<string> GetSongMood(string lyrics, int bpm);
}