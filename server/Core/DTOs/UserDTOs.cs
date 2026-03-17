using System.ComponentModel.DataAnnotations;

namespace Core.DTOs;

public record RegisterData(
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 8, ErrorMessage = "Username must be between 8 and 50 characters long")]
    string Username,
    [Required(ErrorMessage = "First name is required")]
    string FirstName,
    [Required(ErrorMessage = "Last name is required")]
    string LastName,
    [Required(ErrorMessage = "Age is required")]
    [Range(1, 130, ErrorMessage = "Age must be a valid number")]
    int Age,
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email address must be in valid format")]
    string Email,
    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    string Password,
    [Required(ErrorMessage = "Confirmation password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    string ConfirmPassword
);
public record EditUserData(
    [StringLength(50, MinimumLength = 8, ErrorMessage = "Username must be between 8 and 50 characters long")]
    string Username, 
    string FirstName, 
    string LastName,
    [Range(1, 130, ErrorMessage = "Age must be a valid number")]
    int Age
);
public record UserSummaryResponse(int Id, string Username);
public record UserResponse(int Id, string Username, string FirstName, string LastName, int Age, string Email);

