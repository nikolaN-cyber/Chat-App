namespace Core.DTOs;

public record LoginData(string Username, string Password);
public record LoginResponse(int Id, string Username, string FirstName, string LastName, int Age, string Email, string AccessToken);