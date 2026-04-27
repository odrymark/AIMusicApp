using System.Security.Claims;
using Api.DTOs.Request;
using Api.DTOs.Response;
using Api.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;
    private readonly double _jwtExpireMinutes;
    private readonly double _refreshExpireDays;

    public AuthController(IAuthService service, IConfiguration configuration)
    {
        _service = service;

        var jwtSection = configuration.GetSection("Jwt");
        _jwtExpireMinutes = double.Parse(jwtSection["ExpireMinutes"]!);

        var refreshSection = configuration.GetSection("RefreshToken");
        _refreshExpireDays = double.Parse(refreshSection["ExpireDays"]!);
    }

    private void SetJwtCookie(string token)
    {
        var isSecure = Request.IsHttps;
        
        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = isSecure,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddMinutes(_jwtExpireMinutes)
        });
    }

    private void SetRefreshCookie(string refreshToken)
    {
        var isSecure = Request.IsHttps;
        
        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = isSecure,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(_refreshExpireDays)
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] UserLoginReqDto loginReqDto)
    {
        try
        {
            var output = await _service.Login(loginReqDto);

            SetJwtCookie(output.token);
            SetRefreshCookie(output.refreshToken);

            return Ok(new GetMeResDto
            {
                id = output.id,
                username = output.username,
                isAdmin = output.isAdmin
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Invalid login credentials" });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            try
            {
                await _service.Logout(refreshToken);
            }
            catch (KeyNotFoundException)
            {
                //Clear Cookies
            }
        }

        Response.Cookies.Delete("jwt");
        Response.Cookies.Delete("refreshToken");

        return Ok();
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult GetMe()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return BadRequest(new { message = "Invalid user claims" });

        var username = User.Identity?.Name;
        var isAdmin = User.IsInRole("Admin");

        return Ok(new GetMeResDto
        {
            id = userId,
            username = username ?? "Unknown",
            isAdmin = isAdmin
        });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { message = "Missing refresh token" });

        try
        {
            var (token, refresh) = await _service.RefreshToken(refreshToken, _refreshExpireDays);

            SetJwtCookie(token);
            SetRefreshCookie(refresh);

            return Ok();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}