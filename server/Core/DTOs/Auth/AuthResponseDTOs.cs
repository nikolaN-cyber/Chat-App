namespace Core.DTOs.Auth;
public record LoginResponse(int Id, string Username, string FirstName, string LastName, int Age, string Email, string PhotoUrl, string AccessToken);
