using Api.DTOs.Request;
using Api.Services.Auth;
using Api.Services.Password;
using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.User;

public class UserService(IPasswordService passwordService, MusicDbContext context) : IUserService
{
    public async Task CreateUser(UserCreateReqDto userCreateReqDto)
    {
        var username = userCreateReqDto.username;
        var email = userCreateReqDto.email;
        var password = userCreateReqDto.password;
        var passwordConfirm = userCreateReqDto.passwordConfirm;
        
        if (password != passwordConfirm)
            throw new ArgumentException("Passwords do not match.");
        
        var existingUser = await context.Users
            .FirstOrDefaultAsync(u =>
                u.username == username ||
                u.email == email);

        if (existingUser != null)
        {
            if (existingUser.username == username)
                throw new InvalidOperationException("Username already exists.");

            if (existingUser.email == email)
                throw new InvalidOperationException("Email already exists.");
        }

        var hashedPassword = passwordService.HashPassword(userCreateReqDto.password);

        var newUser = new DataAccess.User
        {
            username = username,
            email = email,
            password = hashedPassword,
            isAdmin = false
        };

        context.Users.Add(newUser);
        await context.SaveChangesAsync();
    }
}