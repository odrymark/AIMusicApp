using Api.DTOs.Response;
using Api.Services.AI;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Test.ServiceTests.AiTests;

public class AiStartup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        var aiMock = Substitute.For<IAiService>();

        aiMock.GetSongMood(Arg.Any<string>(), Arg.Any<int>())
            .Returns("happy");

        aiMock.GetRecommendations(Arg.Any<IEnumerable<SongResDto>>(), Arg.Any<IEnumerable<SongResDto>>())
            .Returns(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });

        services.AddSingleton(aiMock);
        services.AddSingleton<IAiService>(aiMock);
    }
}