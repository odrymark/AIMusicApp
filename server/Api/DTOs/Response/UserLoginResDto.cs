namespace Api.DTOs.Response;

public class UserLoginResDto
{
    public Guid id { get; set; }
    public required string username { get; set; }
    public bool isAdmin { get; set; }
    public required string token { get; set; }
    public required string refreshToken { get; set; }
}