namespace MeOne.Application.Communication.Users.Responses;

public sealed record UserResponse(ulong Id, uint Registration, string Name, string? Email, string Department);
