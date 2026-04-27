using Microsoft.AspNetCore.Http;

namespace Api.Services.R2;

public interface IR2Service
{
    Task<string> UploadSongStorage(IFormFile file);
    Task<string> UploadImageStorage(IFormFile file);
    string GenerateSignedUrl(string key, int expirationHours = 1);
    Task DeleteFile(string key);
}