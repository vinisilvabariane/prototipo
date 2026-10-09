namespace IrisWeb.Application.Communication.Users.Responses;

/// <summary><c>Department</c> é o nome do departamento, não o id.</summary>
public sealed record UserResponse(ulong Id, uint Registration, string Name, string? Email, string Department);
