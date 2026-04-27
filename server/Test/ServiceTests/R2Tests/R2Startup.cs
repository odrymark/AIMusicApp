using Amazon.S3;
using Api.Services.R2;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Test.ServiceTests.R2Tests;

public class R2Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        var s3Mock = Substitute.For<IAmazonS3>();
        services.AddSingleton(s3Mock);

        services.AddScoped<IR2Service, R2Service>();
    }
}