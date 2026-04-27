namespace Api.Services.Token;

public interface ITokenService
{
    string GenerateRefreshToken();
    string GenerateToken(DataAccess.User user);
}