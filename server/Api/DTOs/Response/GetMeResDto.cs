namespace Api.DTOs.Response;

public class GetMeResDto
{
    public Guid id { get; set; }
    public required string username { get; set; }
    public bool isAdmin { get; set; }
}