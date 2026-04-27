namespace Api.Services.Password;

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyHashedPassword(string hashedPassword, string providedPassword);
    string HashRefreshToken(string token);
}