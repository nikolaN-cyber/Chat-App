using System.ComponentModel.DataAnnotations;

namespace Core.DTOs;

public record LoginData(
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 8, ErrorMessage = "Username must be between 8 and 50 characters long")]
    string Username,
    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be minimum 8 characters long")]
    string Password
);
public record LoginResponse(int Id, string Username, string FirstName, string LastName, int Age, string Email, string AccessToken);