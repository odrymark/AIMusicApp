using Api.Controllers;
using Api.DTOs.Request;
using Api.DTOs.Response;
using Api.Services.AI;
using Api.Services.Song;
using Api.Services.R2;
using FHHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit.DependencyInjection;

namespace Test.ControllerTests.SongTests;

[Startup(typeof(SongControllerStartup))]
public class SongControllerTests(
    ISongService mockSongService,
    IR2Service mockR2Service,
    IFeatureStateProvider mockStateProvider,
    IAiService mockAiService,
    IServiceProvider provider)
{
    private readonly SongController _controller = SongControllerStartup.GetController(provider);

    [Fact]
    public async Task UploadSong_Calls_R2_For_Both_Song_And_Image()
    {
        var userId = Guid.NewGuid();
        SongControllerStartup.SetupUserClaims(_controller, userId);

        var mockSongFile = Substitute.For<IFormFile>();
        var mockImgFile = Substitute.For<IFormFile>();
        
        var dto = new UploadSongReqDto 
        { 
            title = "New Track", 
            file = mockSongFile, 
            image = mockImgFile, 
            artist = "AI Artist",
            isPublic = true,
            bpm = 123,
        };

        mockR2Service.UploadSongStorage(mockSongFile).Returns("song-key-123");
        mockR2Service.UploadImageStorage(mockImgFile).Returns("img-key-456");
        mockAiService.GetSongMood("", dto.bpm).Returns("happy");

        var result = await _controller.UploadSong(dto);

        Assert.IsType<OkResult>(result);
        await mockR2Service.Received(1).UploadSongStorage(mockSongFile);
        await mockR2Service.Received(1).UploadImageStorage(mockImgFile);
        await mockSongService.Received(1).CreateSong(userId, dto.title, "song-key-123", dto.artist, dto.isPublic, "happy", "img-key-456");
    }

    [Fact]
    public async Task UploadSong_Calls_R2_For_Song_Only_When_No_Image()
    {
        mockSongService.ClearReceivedCalls();
        mockR2Service.ClearReceivedCalls();
    
        var userId = Guid.NewGuid();
        SongControllerStartup.SetupUserClaims(_controller, userId);

        var mockSongFile = Substitute.For<IFormFile>();
    
        var dto = new UploadSongReqDto 
        { 
            title = "Track Without Image", 
            file = mockSongFile, 
            image = null,
            artist = "Artist Name",
            isPublic = false,
            bpm = 123
        };

        mockR2Service.UploadSongStorage(mockSongFile).Returns("song-key-789");
        mockAiService.GetSongMood("", dto.bpm).Returns("happy");

        var result = await _controller.UploadSong(dto);

        Assert.IsType<OkResult>(result);
        await mockR2Service.Received(1).UploadSongStorage(mockSongFile);
        await mockR2Service.DidNotReceive().UploadImageStorage(Arg.Any<IFormFile>());
        await mockSongService.Received(1).CreateSong(userId, dto.title, "song-key-789", dto.artist, dto.isPublic, "happy", null);
    }

    [Fact]
    public async Task UploadSong_Returns_BadRequest_On_Exception()
    {
        var userId = Guid.NewGuid();
        SongControllerStartup.SetupUserClaims(_controller, userId);

        var mockSongFile = Substitute.For<IFormFile>();
        
        var dto = new UploadSongReqDto 
        { 
            title = "Error Track", 
            file = mockSongFile, 
            image = null,
            artist = "Artist",
            isPublic = true,
            bpm = 123
        };

        mockR2Service.UploadSongStorage(mockSongFile).ThrowsAsync(new Exception("Upload failed"));

        var result = await _controller.UploadSong(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetUserSongs_Returns_Songs_For_Logged_In_User()
    {
        var userId = Guid.NewGuid();
        SongControllerStartup.SetupUserClaims(_controller, userId);
        
        var songs = new List<SongResDto> 
        { 
            new() { id = Guid.NewGuid(), title = "Song 1", artist = "Artist Name 1", isPublic = true, songKey = "Song Key 1", mood = "happy" }
        };
        mockSongService.GetUserSongs(userId).Returns(songs);

        var result = await _controller.GetUserSongs();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(songs, okResult.Value);
        await mockSongService.Received(1).GetUserSongs(userId);
    }

    [Fact]
    public async Task GetUserSongs_Returns_BadRequest_On_Exception()
    {
        var userId = Guid.NewGuid();
        SongControllerStartup.SetupUserClaims(_controller, userId);
        mockSongService.GetUserSongs(userId).ThrowsAsync(new Exception("DB Error"));

        var result = await _controller.GetUserSongs();

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetSongs_Returns_All_Songs()
    {
        var songs = new List<SongResDto> 
        { 
            new() { id = Guid.NewGuid(), title = "Public Song 1", artist = "Artist Name 1", isPublic = true, songKey = "Song Key 1", mood = "happy" },
            new() { id = Guid.NewGuid(), title = "Public Song 2", artist = "Artist Name 2", isPublic = true, songKey = "Song Key 2", mood = "happy" }
        };
        mockSongService.GetSongs().Returns(songs);

        var result = await _controller.GetSongs();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(songs, okResult.Value);
    }

    [Fact]
    public async Task GetSongs_Returns_BadRequest_On_Exception()
    {
        mockSongService.GetSongs().ThrowsAsync(new Exception("DB Error"));

        var result = await _controller.GetSongs();

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // -------------------------
    // GetRecommendedSongs Tests
    // -------------------------

    [Fact]
    public async Task GetRecommendedSongs_Returns_Recommended_Songs_For_Logged_In_User()
    {
        var userId = Guid.NewGuid();
        SongControllerStartup.SetupUserClaims(_controller, userId);

        var songs = new List<SongResDto> { new() { id = Guid.NewGuid(), title = "Song 1", mood = "happy", songKey = "key", artist = "artist", isPublic = true } };
        var history = new List<SongResDto> { new() { id = Guid.NewGuid(), title = "History Song", mood = "happy", songKey = "key", artist = "artist", isPublic = true } };
        var recommendedIds = new List<Guid> { songs[0].id };
        var recommendedSongs = new List<SongResDto> { songs[0] };

        mockSongService.GetSongs().Returns(songs);
        mockSongService.GetRecentSongs(userId).Returns(history);
        mockAiService.GetRecommendations(history, songs).Returns(recommendedIds);
        mockSongService.GetSongsById(recommendedIds).Returns(recommendedSongs);

        var result = await _controller.GetRecommendedSongs();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(recommendedSongs, okResult.Value);
    }

    [Fact]
    public async Task GetRecommendedSongs_Falls_Back_To_Random_When_AI_Fails()
    {
        var userId = Guid.NewGuid();
        SongControllerStartup.SetupUserClaims(_controller, userId);

        var songs = new List<SongResDto> { new() { id = Guid.NewGuid(), title = "Song 1", mood = "happy", songKey = "key", artist = "artist", isPublic = true } };
        var history = new List<SongResDto>();
        var randomSongs = new List<SongResDto> { new() { id = Guid.NewGuid(), title = "Random Song", mood = "happy", songKey = "key", artist = "artist", isPublic = true } };

        mockSongService.GetSongs().Returns(songs);
        mockSongService.GetRecentSongs(userId).Returns(history);
        mockAiService.GetRecommendations(Arg.Any<IEnumerable<SongResDto>>(), Arg.Any<IEnumerable<SongResDto>>())
            .ThrowsAsync(new Exception("AI failed"));
        mockSongService.GetRandomSongs().Returns(randomSongs);

        var result = await _controller.GetRecommendedSongs();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(randomSongs, okResult.Value);
    }

    [Fact]
    public async Task GetRecommendedSongs_Returns_Random_Songs_When_Not_Logged_In()
    {
        _controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal();

        var randomSongs = new List<SongResDto> { new() { id = Guid.NewGuid(), title = "Random Song", mood = "happy", songKey = "key", artist = "artist", isPublic = true } };
        mockSongService.GetRandomSongs().Returns(randomSongs);

        var result = await _controller.GetRecommendedSongs();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(randomSongs, okResult.Value);
    }

    // -------------------------
    // AddHistory Tests
    // -------------------------

    [Fact]
    public async Task AddHistory_Returns_Ok_When_Successful()
    {
        var userId = Guid.NewGuid();
        var songId = Guid.NewGuid();
        SongControllerStartup.SetupUserClaims(_controller, userId);

        mockSongService.AddHistory(userId, songId).Returns(Task.CompletedTask);

        var result = await _controller.AddHistory(songId.ToString());

        Assert.IsType<OkResult>(result);
        await mockSongService.Received(1).AddHistory(userId, songId);
    }

    [Fact]
    public async Task AddHistory_Returns_BadRequest_On_Exception()
    {
        var userId = Guid.NewGuid();
        SongControllerStartup.SetupUserClaims(_controller, userId);

        mockSongService.AddHistory(Arg.Any<Guid>(), Arg.Any<Guid>())
            .ThrowsAsync(new Exception("DB Error"));

        var result = await _controller.AddHistory(Guid.NewGuid().ToString());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // -------------------------
    // GetSignedUrl Tests
    // -------------------------

    [Fact]
    public void GetSignedUrl_Returns_Ok_With_Url()
    {
        mockR2Service.ClearReceivedCalls();
        mockR2Service.GenerateSignedUrl(Arg.Any<string>()).Returns("https://signed-url.com");

        var result = _controller.GetSignedUrl("some-file-key");
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("https://signed-url.com", okResult.Value);
    }

    [Fact]
    public void GetSignedUrl_Returns_BadRequest_On_Exception()
    {
        mockR2Service.GenerateSignedUrl(Arg.Any<string>()).Throws(new Exception("Invalid key"));

        var result = _controller.GetSignedUrl("bad-key");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // -------------------------
    // EditSong Tests
    // -------------------------

    [Fact]
    public async Task EditSong_Returns_Forbidden_When_Feature_Disabled()
    {
        var userId = Guid.NewGuid();
        SongControllerStartup.SetupUserClaims(_controller, userId);

        mockStateProvider.IsEnabled("edit_song").Returns(false);

        var dto = new SongEditReqDto 
        { 
            id = Guid.NewGuid(),
            title = "Updated Title",
            image = null,
            prevImgKey = null,
            artist = "TestArtist",
            isPublic = true
        };

        var result = await _controller.EditSong(dto);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
    }

    [Fact]
    public async Task EditSong_Updates_Song_When_Feature_Enabled_Without_Image()
    {
        mockSongService.ClearReceivedCalls();
        mockR2Service.ClearReceivedCalls();
        mockStateProvider.ClearReceivedCalls();

        var userId = Guid.NewGuid();
        var songId = Guid.NewGuid();
        SongControllerStartup.SetupUserClaims(_controller, userId);

        mockStateProvider.IsEnabled("edit_song").Returns(true);

        var dto = new SongEditReqDto 
        { 
            id = songId,
            title = "Updated Title",
            image = null,
            prevImgKey = null,
            artist = "Updated Artist",
            isPublic = false
        };

        mockSongService.EditSong(userId, songId, dto.title, dto.artist, dto.isPublic, null)
            .Returns(Task.CompletedTask);

        var result = await _controller.EditSong(dto);

        Assert.IsType<OkResult>(result);
        await mockSongService.Received(1).EditSong(userId, songId, dto.title, dto.artist, dto.isPublic, null);
        await mockR2Service.DidNotReceive().DeleteFile(Arg.Any<string>());
    }

    [Fact]
    public async Task EditSong_Updates_Song_And_Image_When_Feature_Enabled()
    {
        mockSongService.ClearReceivedCalls();
        mockR2Service.ClearReceivedCalls();
        mockStateProvider.ClearReceivedCalls();

        var userId = Guid.NewGuid();
        var songId = Guid.NewGuid();
        SongControllerStartup.SetupUserClaims(_controller, userId);

        mockStateProvider.IsEnabled("edit_song").Returns(true);

        var mockImgFile = Substitute.For<IFormFile>();
        var dto = new SongEditReqDto 
        { 
            id = songId,
            title = "Updated Title",
            image = mockImgFile,
            prevImgKey = "old-image-key",
            artist = "Updated Artist",
            isPublic = true
        };

        mockR2Service.UploadImageStorage(mockImgFile).Returns("new-image-key");
        mockSongService.EditSong(userId, songId, dto.title, dto.artist, dto.isPublic, "new-image-key")
            .Returns(Task.CompletedTask);

        var result = await _controller.EditSong(dto);

        Assert.IsType<OkResult>(result);
        await mockR2Service.Received(1).UploadImageStorage(mockImgFile);
        await mockR2Service.Received(1).DeleteFile("old-image-key");
        await mockSongService.Received(1).EditSong(userId, songId, dto.title, dto.artist, dto.isPublic, "new-image-key");
    }

    [Fact]
    public async Task EditSong_Returns_BadRequest_On_Exception()
    {
        var userId = Guid.NewGuid();
        SongControllerStartup.SetupUserClaims(_controller, userId);

        mockStateProvider.IsEnabled("edit_song").Returns(true);

        var dto = new SongEditReqDto 
        { 
            id = Guid.NewGuid(),
            title = "Updated Title",
            image = null,
            prevImgKey = null,
            artist = "Artist",
            isPublic = true
        };

        mockSongService.EditSong(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>())
            .ThrowsAsync(new Exception("Edit failed"));

        var result = await _controller.EditSong(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }
}