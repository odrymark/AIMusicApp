using Api.Services.Song;
using DataAccess;
using Xunit.DependencyInjection;

namespace Test.ServiceTests.SongTests;

[Startup(typeof(SongStartup))]
public class SongServiceTests(MusicDbContext db, ISongService songService) : TestBase(db)
{
    // -------------------------
    // CreateSong Tests
    // -------------------------

    [Fact]
    public async Task CreateSong_Saves_Song_To_Db()
    {
        var user = await CreateUserAsync("song_create_" + Guid.NewGuid().ToString("N"));

        await songService.CreateSong(user.id, "My Song", "song-key", "Artist", true);

        var song = Db.Songs.FirstOrDefault(s => s.userId == user.id);
        Assert.NotNull(song);
    }

    [Fact]
    public async Task CreateSong_Saves_Correct_Fields()
    {
        var user = await CreateUserAsync("song_fields_" + Guid.NewGuid().ToString("N"));

        await songService.CreateSong(user.id, "My Song", "song-key", "Artist", true, "image.jpg");

        var song = Db.Songs.First(s => s.userId == user.id);
        Assert.Equal("My Song", song.title);
        Assert.Equal("song-key", song.songKey);
        Assert.Equal("Artist", song.artist);
        Assert.True(song.isPublic);
        Assert.Equal("image.jpg", song.image);
    }

    [Fact]
    public async Task CreateSong_Image_Is_Null_When_Not_Provided()
    {
        var user = await CreateUserAsync("song_noimage_" + Guid.NewGuid().ToString("N"));

        await songService.CreateSong(user.id, "My Song", "song-key", "Artist", false);

        var song = Db.Songs.First(s => s.userId == user.id);
        Assert.Null(song.image);
    }

    [Fact]
    public async Task CreateSong_Throws_When_Title_Empty()
    {
        var user = await CreateUserAsync("song_notitle_" + Guid.NewGuid().ToString("N"));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            songService.CreateSong(user.id, "", "song-key", "Artist", true));
    }

    [Fact]
    public async Task CreateSong_Throws_When_Title_Whitespace()
    {
        var user = await CreateUserAsync("song_wstitle_" + Guid.NewGuid().ToString("N"));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            songService.CreateSong(user.id, "   ", "song-key", "Artist", true));
    }

    [Fact]
    public async Task CreateSong_Throws_When_SongKey_Empty()
    {
        var user = await CreateUserAsync("song_nokey_" + Guid.NewGuid().ToString("N"));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            songService.CreateSong(user.id, "My Song", "", "Artist", true));
    }

    [Fact]
    public async Task CreateSong_Throws_When_SongKey_Whitespace()
    {
        var user = await CreateUserAsync("song_wskey_" + Guid.NewGuid().ToString("N"));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            songService.CreateSong(user.id, "My Song", "   ", "Artist", true));
    }

    // -------------------------
    // GetUserSongsAsync Tests
    // -------------------------

    [Fact]
    public async Task GetUserSongsAsync_Returns_Songs_For_User()
    {
        var user = await CreateUserAsync("song_getuser_" + Guid.NewGuid().ToString("N"));
        await CreateSongAsync(user.id, "Song 1");
        await CreateSongAsync(user.id, "Song 2");

        var result = await songService.GetUserSongs(user.id);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetUserSongsAsync_Returns_Only_Users_Own_Songs()
    {
        var user1 = await CreateUserAsync("song_own1_" + Guid.NewGuid().ToString("N"));
        var user2 = await CreateUserAsync("song_own2_" + Guid.NewGuid().ToString("N"));
        await CreateSongAsync(user1.id, "User1 Song");
        await CreateSongAsync(user2.id, "User2 Song");

        var result = await songService.GetUserSongs(user1.id);

        Assert.All(result, s => Assert.Equal("User1 Song", s.title));
    }

    [Fact]
    public async Task GetUserSongsAsync_Returns_Empty_When_No_Songs()
    {
        var user = await CreateUserAsync("song_empty_" + Guid.NewGuid().ToString("N"));

        var result = await songService.GetUserSongs(user.id);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserSongsAsync_Returns_Correct_Dto_Fields()
    {
        var user = await CreateUserAsync("song_dto_" + Guid.NewGuid().ToString("N"));
        var song = await CreateSongAsync(user.id, "DTO Song", "dto-key", "DTO Artist", image: "img.jpg");

        var result = await songService.GetUserSongs(user.id);
        var dto = result.First();

        Assert.Equal(song.id, dto.id);
        Assert.Equal(song.title, dto.title);
        Assert.Equal(song.songKey, dto.songKey);
        Assert.Equal(song.artist, dto.artist);
        Assert.Equal(song.image, dto.image);
    }

    [Fact]
    public async Task GetUserSongsAsync_Throws_When_UserId_Empty()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            songService.GetUserSongs(Guid.Empty));
    }

    // -------------------------
    // GetSongs Tests
    // -------------------------

    [Fact]
    public async Task GetSongs_Returns_Only_Public_Songs()
    {
        var user = await CreateUserAsync("song_public_" + Guid.NewGuid().ToString("N"));
        await CreateSongAsync(user.id, "Public Song", isPublic: true);
        await CreateSongAsync(user.id, "Private Song", isPublic: false);

        var result = await songService.GetSongs();

        Assert.DoesNotContain(result, s => s.title == "Private Song");
        Assert.Contains(result, s => s.title == "Public Song");
    }

    [Fact]
    public async Task GetSongs_Returns_Empty_When_No_Public_Songs()
    {
        var user = await CreateUserAsync("song_nopublic_" + Guid.NewGuid().ToString("N"));
        await CreateSongAsync(user.id, "Private Song", isPublic: false);

        var result = await songService.GetSongs();

        Assert.DoesNotContain(result, s => s.title == "Private Song");
    }

    [Fact]
    public async Task GetSongs_Returns_Correct_Dto_Fields()
    {
        var user = await CreateUserAsync("song_pubdto_" + Guid.NewGuid().ToString("N"));
        var song = await CreateSongAsync(user.id, "Public DTO Song", "pub-key", "Pub Artist", isPublic: true, image: "pub.jpg");

        var result = await songService.GetSongs();
        var dto = result.First(s => s.id == song.id);

        Assert.Equal(song.title, dto.title);
        Assert.Equal(song.songKey, dto.songKey);
        Assert.Equal(song.artist, dto.artist);
        Assert.Equal(song.image, dto.image);
    }
}