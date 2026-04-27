using Api.Services.Auth;
using Api.Services.Password;
using Api.Services.Token;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Test.ServiceTests.AuthTests;

public class AuthStartup
{
    private static DbContainer? _container;

    public static void ConfigureServices(IServiceCollection services)
    {
        if (_container == null)
        {
            _container = new DbContainer();
            _container.InitializeAsync().AsTask().GetAwaiter().GetResult();
        }

        services.AddSingleton(_container);
        
        services.AddDbContext<MusicDbContext>(options =>
            options.UseNpgsql(_container.Container.GetConnectionString()));
        
        var tokenMock = Substitute.For<ITokenService>();
        tokenMock.GenerateToken(Arg.Any<User>())
            .Returns("jwt_token");

        tokenMock.GenerateRefreshToken()
            .Returns("refresh_token_new");

        var passwordMock = Substitute.For<IPasswordService>();
        passwordMock.HashRefreshToken(Arg.Any<string>())
            .Returns(ci => "HASH_" + ci.Arg<string>());
        passwordMock.VerifyHashedPassword(Arg.Any<string>(), Arg.Any<string>())
            .Returns(true);

        services.AddSingleton(tokenMock);
        services.AddSingleton(passwordMock);
        
        services.AddScoped<IAuthService, AuthService>();
    }
}