using System.Net;
using System.Text.Json;
using Api.DTOs.Response;
using Api.Services.AI;
using NSubstitute;
using Xunit;

namespace Test.ServiceTests.AiTests;

public class AiServiceTests
{
    private AiService CreateService(HttpResponseMessage response)
    {
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://ai_backend:8000") };
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("AiBackend").Returns(httpClient);
        return new AiService(factory);
    }

    private HttpResponseMessage JsonResponse(object body, HttpStatusCode status = HttpStatusCode.OK)
    {
        var json = JsonSerializer.Serialize(body);
        return new HttpResponseMessage(status)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };
    }

    private SongResDto MakeSong(string mood = "happy") => new()
    {
        id = Guid.NewGuid(),
        title = "Test Song",
        artist = "Test Artist",
        songKey = "test-key",
        mood = mood,
        isPublic = true
    };

    // -------------------------
    // GetSongMood Tests
    // -------------------------

    [Fact]
    public async Task GetSongMood_Returns_Mood_From_Response()
    {
        var service = CreateService(JsonResponse(new { mood = "happy" }));

        var result = await service.GetSongMood("happy lyrics", 120);

        Assert.Equal("happy", result);
    }

    [Fact]
    public async Task GetSongMood_Returns_Sad_Mood()
    {
        var service = CreateService(JsonResponse(new { mood = "sad" }));

        var result = await service.GetSongMood("sad lyrics", 60);

        Assert.Equal("sad", result);
    }

    [Fact]
    public async Task GetSongMood_Throws_When_Backend_Returns_Error()
    {
        var service = CreateService(JsonResponse(new { }, HttpStatusCode.InternalServerError));

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GetSongMood("some lyrics", 120));
    }

    [Fact]
    public async Task GetSongMood_Throws_When_Backend_Returns_NotFound()
    {
        var service = CreateService(JsonResponse(new { }, HttpStatusCode.NotFound));

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GetSongMood("some lyrics", 120));
    }

    // -------------------------
    // GetRecommendations Tests
    // -------------------------

    [Fact]
    public async Task GetRecommendations_Returns_Song_Ids()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var service = CreateService(JsonResponse(new { song_ids = new[] { id1.ToString(), id2.ToString() } }));

        var result = await service.GetRecommendations(
            new[] { MakeSong("happy") },
            new[] { MakeSong("happy"), MakeSong("sad") }
        );

        Assert.Equal(2, result.Count());
        Assert.Contains(id1, result);
        Assert.Contains(id2, result);
    }

    [Fact]
    public async Task GetRecommendations_Returns_Empty_When_No_Ids()
    {
        var service = CreateService(JsonResponse(new { song_ids = Array.Empty<string>() }));

        var result = await service.GetRecommendations(
            new[] { MakeSong() },
            new[] { MakeSong() }
        );

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecommendations_Throws_When_Backend_Returns_Error()
    {
        var service = CreateService(JsonResponse(new { }, HttpStatusCode.InternalServerError));

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GetRecommendations(
                new[] { MakeSong() },
                new[] { MakeSong() }
            ));
    }

    [Fact]
    public async Task GetRecommendations_Returns_Single_Id()
    {
        var id = Guid.NewGuid();
        var service = CreateService(JsonResponse(new { song_ids = new[] { id.ToString() } }));

        var result = await service.GetRecommendations(
            new[] { MakeSong("romantic") },
            new[] { MakeSong("romantic") }
        );

        Assert.Single(result);
        Assert.Equal(id, result.First());
    }

    [Fact]
    public async Task GetRecommendations_Sends_Correct_Moods()
    {
        var id = Guid.NewGuid();
        var handler = new CapturingHttpMessageHandler(JsonResponse(new { song_ids = new[] { id.ToString() } }));
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://ai_backend:8000") };
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("AiBackend").Returns(httpClient);
        var service = new AiService(factory);

        await service.GetRecommendations(
            new[] { MakeSong("happy"), MakeSong("sad") },
            new[] { MakeSong("energetic") }
        );

        var body = await handler.LastRequest!.Content!.ReadAsStringAsync();
        Assert.Contains("happy", body);
        Assert.Contains("sad", body);
    }
}

public class MockHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(response);
}

public class CapturingHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(response);
    }
}