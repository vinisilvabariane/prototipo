namespace MeOne.Application.Communication.Users.Requests;

public sealed record ChangePasswordRequest(ulong UserId, string NewPassword);
