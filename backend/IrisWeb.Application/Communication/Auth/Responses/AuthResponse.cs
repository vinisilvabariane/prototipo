namespace IrisWeb.Application.Communication.Auth.Responses;

public sealed record AuthResponse(string Token, string RefreshToken, DateTime RefreshExpires);
