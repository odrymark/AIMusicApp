using Api.Services.Auth;
using Api.Services.Password;
using Api.Services.User;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Test.ServiceTests.UserTests;

public class UserStartup
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

        var passwordMock = Substitute.For<IPasswordService>();
        passwordMock.HashPassword(Arg.Any<string>())
            .Returns(ci => "HASH_" + ci.Arg<string>());

        var authMock = Substitute.For<IAuthService>();

        services.AddSingleton(passwordMock);
        services.AddSingleton(authMock);

        services.AddScoped<IUserService, UserService>();
    }
}