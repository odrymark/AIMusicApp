using Api.Services.Password;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
 
namespace Test.SeederTests;
 
public class SeederStartup
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
 
        services.AddSingleton(passwordMock);
    }
}