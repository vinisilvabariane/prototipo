namespace MeOne.Application.Communication.Users.Requests;

public sealed record ConfirmPasswordResetRequest(uint Registration, string Code, string NewPassword);
