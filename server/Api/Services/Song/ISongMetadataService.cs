namespace Api.Services.Song;

public interface ISongMetadataService
{
    Task<string> GetMetadataAsync(string title, string artist);
}