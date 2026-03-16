namespace Core.DTOs;

public record RegisterData(string Username, string FirstName, string LastName, int Age, string Email, string Password, string ConfirmPassword);
public record EditUserData(string Username, string FirstName, string LastName, int Age);
public record UserSummaryResponse(int Id, string Username);
public record UserResponse(int Id, string Username, string FirstName, string LastName, int Age, string Email);

