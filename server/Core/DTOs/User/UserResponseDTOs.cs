namespace Core.DTOs.User;

public record UserSummaryResponse(int Id, string Username);
public record UserResponse(int Id, string Username, string FirstName, string LastName, int Age, string Email);