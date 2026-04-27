using Api.Controllers;
using Api.DTOs.Request;
using Api.DTOs.Response;
using Api.Services.Playlist;
using Api.Services.R2;
using FHHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit.DependencyInjection;

namespace Test.ControllerTests.PlaylistTests;

[Startup(typeof(PlaylistControllerStartup))]
public class PlaylistControllerTests(
    IPlaylistService mockPlaylistService,
    IR2Service mockR2Service,
    IFeatureStateProvider mockStateProvider,
    IServiceProvider provider)
{
    private readonly PlaylistController _controller = PlaylistControllerStartup.GetController(provider);

    [Fact]
    public async Task GetUserPlaylists_Returns_Ok_With_User_Id_From_Claims()
    {
        var userId = Guid.NewGuid();
        PlaylistControllerStartup.SetupUserClaims(_controller, userId);
        
        mockPlaylistService.GetUserPlaylists(userId).Returns(new List<PlaylistResDto>());

        var result = await _controller.GetUserPlaylists();

        Assert.IsType<OkObjectResult>(result);
        await mockPlaylistService.Received(1).GetUserPlaylists(userId);
    }

    [Fact]
    public async Task CreatePlaylist_Uploads_Image_If_Provided()
    {
        var userId = Guid.NewGuid();
        PlaylistControllerStartup.SetupUserClaims(_controller, userId);

        var mockFile = Substitute.For<IFormFile>();
        mockFile.FileName.Returns("test.png");
        mockFile.Length.Returns(1024);

        var dto = new PlaylistCreateReqDto 
        { 
            title = "My Jam", 
            image = mockFile, 
            songIds = new List<Guid>(), 
            isPublic = true 
        };

        mockR2Service.UploadImageStorage(mockFile).Returns("generated-image-key");

        var result = await _controller.CreatePlaylist(dto);

        Assert.IsType<OkResult>(result);
        await mockR2Service.Received(1).UploadImageStorage(mockFile);
        await mockPlaylistService.Received(1).CreatePlaylist(userId, dto.title, dto.songIds, dto.isPublic, "generated-image-key");
    }

    [Fact]
    public async Task EditPlaylist_Deletes_Old_Image_When_New_One_Uploaded()
    {
        var userId = Guid.NewGuid();
        PlaylistControllerStartup.SetupUserClaims(_controller, userId);

        var mockFile = Substitute.For<IFormFile>();
        var dto = new PlaylistEditReqDto 
        { 
            id = Guid.NewGuid(),
            title = "Updated", 
            image = mockFile, 
            prevImgKey = "old-key",
            songIds = new List<Guid>(), 
            isPublic = true
        };

        mockStateProvider.IsEnabled("edit_playlist").Returns(true);
        mockR2Service.UploadImageStorage(mockFile).Returns("new-key");

        await _controller.EditPlaylist(dto);

        await mockR2Service.Received(1).DeleteFile("old-key");
        await mockPlaylistService.Received(1).EditPlaylist(userId, dto.id, dto.title, Arg.Any<List<Guid>>(), Arg.Any<bool>(), "new-key");
    }
}