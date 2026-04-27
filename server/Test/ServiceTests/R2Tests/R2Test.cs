using Api.Services.R2;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace Test.ServiceTests.R2Tests;

public class R2ServiceTests
{
    private readonly R2Service _r2Service;

    public R2ServiceTests()
    {
        var mockConfig = CreateMockConfig("test-access", "test-secret", "test-bucket", "https://test-endpoint.com");
        _r2Service = new R2Service(mockConfig);
    }

    private static IConfiguration CreateMockConfig(string? access = null, string? secret = null, string? bucket = null, string? endpoint = null)
    {
        var config = Substitute.For<IConfiguration>();
        config["R2:AccessKey"].Returns(access);
        config["R2:SecretKey"].Returns(secret);
        config["R2:BucketName"].Returns(bucket);
        config["R2:Endpoint"].Returns(endpoint);
        return config;
    }

    private static IFormFile CreateFormFile(string fileName, string contentType = "audio/mpeg", int size = 1024)
    {
        var file = Substitute.For<IFormFile>();
        file.FileName.Returns(fileName);
        file.ContentType.Returns(contentType);
        file.Length.Returns(size);
        file.OpenReadStream().Returns(new MemoryStream(new byte[size]));
        return file;
    }

    [Theory]
    [InlineData(null, "secret", "bucket", "endpoint", "AccessKey")]
    [InlineData("access", null, "bucket", "endpoint", "SecretKey")]
    [InlineData("access", "secret", null, "endpoint", "BucketName")]
    [InlineData("access", "secret", "bucket", null, "Endpoint")]
    public void Constructor_Throws_When_Keys_Are_Missing(string? a, string? s, string? b, string? e, string expectedMsg)
    {
        var badConfig = CreateMockConfig(a, s, b, e);
        var exception = Assert.Throws<InvalidOperationException>(() => new R2Service(badConfig));
        Assert.Contains(expectedMsg, exception.Message);
    }

    [Fact]
    public void Constructor_Throws_When_Endpoint_Is_Whitespace()
    {
        var badConfig = CreateMockConfig("access", "secret", "bucket", "   ");
        var exception = Assert.Throws<InvalidOperationException>(() => new R2Service(badConfig));
        Assert.Contains("Endpoint is empty", exception.Message);
    }

    [Fact]
    public async Task UploadSongStorage_Throws_When_File_Is_Null()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _r2Service.UploadSongStorage(null!));
    }

    [Fact]
    public async Task UploadSongStorage_Throws_When_File_Is_Empty()
    {
        var file = CreateFormFile("song.mp3", size: 0);
        await Assert.ThrowsAsync<ArgumentException>(() => _r2Service.UploadSongStorage(file));
    }

    [Fact]
    public async Task UploadSongStorage_Throws_When_Invalid_Extension()
    {
        var file = CreateFormFile("song.exe", "application/octet-stream");
        await Assert.ThrowsAsync<ArgumentException>(() => _r2Service.UploadSongStorage(file));
    }
    
    [Fact]
    public async Task UploadSongStorage_Accepts_Mp3_Extension()
    {
        var file = CreateFormFile("song.mp3");
        await Assert.ThrowsAsync<HttpRequestException>(() => _r2Service.UploadSongStorage(file));
    }

    [Fact]
    public void GenerateSignedUrl_Returns_Url_String()
    {
        var result = _r2Service.GenerateSignedUrl("some-file-key.mp3");

        Assert.NotNull(result);
        Assert.StartsWith("https://", result);
        Assert.Contains("test-bucket", result);
    }

    [Fact]
    public void GenerateSignedUrl_Returns_Different_Urls_For_Different_Keys()
    {
        var url1 = _r2Service.GenerateSignedUrl("key1.mp3");
        var url2 = _r2Service.GenerateSignedUrl("key2.mp3");

        Assert.NotEqual(url1, url2);
    }

    [Fact]
    public async Task DeleteFile_Returns_Early_When_Key_Is_Null_Or_Empty()
    {
        var exceptionNull = await Record.ExceptionAsync(() => _r2Service.DeleteFile(null!));
        var exceptionEmpty = await Record.ExceptionAsync(() => _r2Service.DeleteFile(""));
        var exceptionWhitespace = await Record.ExceptionAsync(() => _r2Service.DeleteFile("   "));

        Assert.Null(exceptionNull);
        Assert.Null(exceptionEmpty);
        Assert.Null(exceptionWhitespace);
    }

    [Fact]
    public async Task DeleteFile_Attempts_Delete_With_Valid_Key()
    {
        await Assert.ThrowsAsync<HttpRequestException>(() => _r2Service.DeleteFile("songs/file.mp3"));
    }
}