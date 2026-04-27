using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Request;

public class UserCreateReqDto
{
    [Required(ErrorMessage = "Username is required.")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters.")]
    [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$",
        ErrorMessage = "Username can only contain alphanumeric characters and underscores.")]
    public required string username { get; set; }
    
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public required string email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    public required string password { get; set; }
    
    [Required(ErrorMessage = "Password confirmation is required.")]
    public required string passwordConfirm { get; set; }
}