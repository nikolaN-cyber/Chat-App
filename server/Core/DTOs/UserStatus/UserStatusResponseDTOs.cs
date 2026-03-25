namespace Core.DTOs.UserStatus;

public record StatusResponse(string Emoji, string Status);

public record StatusStatisticsResponse(TimeSpan OnVacation, TimeSpan WorkingRemotely);