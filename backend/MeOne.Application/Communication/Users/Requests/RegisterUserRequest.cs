namespace MeOne.Application.Communication.Users.Requests;

public sealed record RegisterUserRequest(
    string Name,
    string Email,
    uint Registration,
    string Password,
    string Department);
