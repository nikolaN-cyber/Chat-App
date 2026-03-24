using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.User;

public record EditUserData(
    [StringLength(50, MinimumLength = 8, ErrorMessage = "Username must be between 8 and 50 characters long")]
    string Username,
    string FirstName,
    string LastName,
    [Range(1, 130, ErrorMessage = "Age must be a valid number")]
    int Age
);


