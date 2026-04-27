using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Api.Services.Token;
using DataAccess;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using Xunit.DependencyInjection;

namespace Test.ServiceTests.TokenTests;

[Startup(typeof(TokenStartup))]
public class TokenServiceTests(MusicDbContext db, ITokenService tokenService, IConfiguration configuration)
    : TestBase(db)
{
    private static User BuildUser(bool isAdmin = false) => new()
    {
        id = Guid.NewGuid(),
        username = "testuser_" + Guid.NewGuid().ToString("N"),
        password = "hashed_pw",
        email = "test@example.com",
        isAdmin = isAdmin
    };

    private JwtSecurityToken ParseToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        handler.InboundClaimTypeMap.Clear();
        var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!);

        handler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        }, out var validated);

        return (JwtSecurityToken)validated;
    }

    // -------------------------
    // GenerateRefreshToken Tests
    // -------------------------

    [Fact]
    public void GenerateRefreshToken_Returns_NonEmpty_String()
    {
        var result = tokenService.GenerateRefreshToken();

        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public void GenerateRefreshToken_Returns_Valid_Base64()
    {
        var result = tokenService.GenerateRefreshToken();

        var decoded = Convert.FromBase64String(result);
        Assert.Equal(64, decoded.Length);
    }

    [Fact]
    public void GenerateRefreshToken_Returns_Unique_Values()
    {
        var token1 = tokenService.GenerateRefreshToken();
        var token2 = tokenService.GenerateRefreshToken();

        Assert.NotEqual(token1, token2);
    }

    // -------------------------
    // GenerateToken Tests
    // -------------------------

    [Fact]
    public void GenerateToken_Returns_NonEmpty_String()
    {
        var result = tokenService.GenerateToken(BuildUser());

        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public void GenerateToken_Returns_Valid_Jwt()
    {
        var user = BuildUser();

        var token = tokenService.GenerateToken(user);
        var parsed = ParseToken(token);

        Assert.NotNull(parsed);
    }

    [Fact]
    public void GenerateToken_Contains_User_Id_Claim()
    {
        var user = BuildUser();

        var token = tokenService.GenerateToken(user);
        var parsed = ParseToken(token);

        var sub = parsed.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        Assert.Equal(user.id.ToString(), sub);
    }

    [Fact]
    public void GenerateToken_Contains_Username_Claim()
    {
        var user = BuildUser();

        var token = tokenService.GenerateToken(user);
        var parsed = ParseToken(token);

        var username = parsed.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName)?.Value;
        Assert.Equal(user.username, username);
    }

    [Fact]
    public void GenerateToken_Contains_User_Role_When_Not_Admin()
    {
        var user = BuildUser(isAdmin: false);

        var token = tokenService.GenerateToken(user);
        var parsed = ParseToken(token);

        var role = parsed.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
        Assert.Equal("User", role);
    }

    [Fact]
    public void GenerateToken_Contains_Admin_Role_When_Admin()
    {
        var user = BuildUser(isAdmin: true);

        var token = tokenService.GenerateToken(user);
        var parsed = ParseToken(token);

        var role = parsed.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
        Assert.Equal("Admin", role);
    }

    [Fact]
    public void GenerateToken_Has_Correct_Issuer_And_Audience()
    {
        var token = tokenService.GenerateToken(BuildUser());
        var parsed = ParseToken(token);

        Assert.Equal("test_issuer", parsed.Issuer);
        Assert.Contains("test_audience", parsed.Audiences);
    }

    [Fact]
    public void GenerateToken_Expires_After_Configured_Minutes()
    {
        var user = BuildUser();
        var before = DateTime.UtcNow.AddMinutes(14);
        var after = DateTime.UtcNow.AddMinutes(16);

        var token = tokenService.GenerateToken(user);
        var parsed = ParseToken(token);

        Assert.True(parsed.ValidTo > before && parsed.ValidTo < after);
    }

    [Fact]
    public void GenerateToken_Returns_Different_Tokens_For_Different_Users()
    {
        var token1 = tokenService.GenerateToken(BuildUser());
        var token2 = tokenService.GenerateToken(BuildUser());

        Assert.NotEqual(token1, token2);
    }
}