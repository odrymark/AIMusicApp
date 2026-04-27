using Api.DTOs.Request;
using Api.DTOs.Response;
using Api.Services;
using Api.Services.User;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/user")]
public class UserController(IUserService service) : ControllerBase
{
    [HttpPost("createUser")]
    public async Task<ActionResult> CreateUser([FromBody] UserCreateReqDto userCreateReqDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors) });

        try
        {
            await service.CreateUser(userCreateReqDto);
            return Created(nameof(CreateUser), new { message = "User created successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}