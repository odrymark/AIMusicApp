using Api.DTOs.Request;

namespace Api.Services.User;

public interface IUserService
{
    Task CreateUser(UserCreateReqDto userCreateReqDto);
}