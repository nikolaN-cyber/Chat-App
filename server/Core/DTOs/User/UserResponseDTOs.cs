using Core.DTOs.UserStatus;

namespace Core.DTOs.User;

public record UserSummaryResponse(int Id, string Username, string PhotoUrl, StatusResponse Status);
public record UserResponse(int Id, string Username, string FirstName, string LastName, int Age, string Email);