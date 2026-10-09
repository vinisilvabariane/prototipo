namespace IrisWeb.Application.Communication.Users.Requests;

public sealed record UpdateUserRequest(string Name, string? Email, uint Registration, string Department);
