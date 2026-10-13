namespace MeOne.Application.Communication.Users.Responses;

public sealed record ResetPasswordResponse(string? NewPassword, bool EmailSent);
