using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Api.DTOs.Request;
using Api.Services;
using Api.Services.R2;
using Api.Services.Song;
using FHHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/song")]
public class SongController(IR2Service r2Service, ISongService songService, IFeatureStateProvider stateProvider) : ControllerBase
{
    [Authorize]
    [HttpPost("uploadSong")]
    public async Task<IActionResult> UploadSong([FromForm] UploadSongReqDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var id = Guid.Parse(idStr!);

            var songKey = await r2Service.UploadSongStorage(dto.file);
        
            string? imgKey = null;
            if (dto.image != null)
            {
                imgKey = await r2Service.UploadImageStorage(dto.image);
            }

            await songService.CreateSong(id, dto.title, songKey, dto.artist, dto.isPublic, imgKey);

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex}");
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpGet("getUserSongs")]
    public async Task<IActionResult> GetUserSongs()
    {
        try
        {
            var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var id = Guid.Parse(idStr!);
            
            var songs = await songService.GetUserSongs(id);
            
            return Ok(songs);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("getSongs")]
    public async Task<IActionResult> GetSongs()
    {
        try
        {
            var songs = await songService.GetSongs();
            return Ok(songs);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("getSignedUrl")]
    public IActionResult GetSignedUrl(string key)
    {
        try
        {
            var url = r2Service.GenerateSignedUrl(key);
            return Ok(url);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpPost("editSong")]
    public async Task<IActionResult> EditSong([FromForm] SongEditReqDto dto)
    {
        if (!stateProvider.IsEnabled("edit_song"))
            return Problem(
                detail: "Song editing is currently not available",
                statusCode: StatusCodes.Status403Forbidden,
                title: "Feature Disabled"
            );
        
        try
        {
            var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var id = Guid.Parse(idStr!);

            string? imgKey = null;
            if (dto.image != null)
            {
                imgKey = await r2Service.UploadImageStorage(dto.image);
                
                await r2Service.DeleteFile(dto.prevImgKey!);
            }
            
            await songService.EditSong(id, dto.id, dto.title, dto.artist, dto.isPublic, imgKey);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}