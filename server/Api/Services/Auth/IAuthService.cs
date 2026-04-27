using Api.DTOs.Request;
using Api.DTOs.Response;

namespace Api.Services.Auth;

public interface IAuthService
{
    Task<UserLoginResDto> Login(UserLoginReqDto userLoginReqDto);
    Task<(string token, string refresh)> RefreshToken(string refreshToken, double refreshTokenDays);
    Task Logout(string refreshToken);
}