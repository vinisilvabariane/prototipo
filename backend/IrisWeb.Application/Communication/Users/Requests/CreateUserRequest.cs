namespace IrisWeb.Application.Communication.Users.Requests;

public sealed record CreateUserRequest(
    uint Registration,
    string Name,
    string? Email,
    string Password,
    string Department);
