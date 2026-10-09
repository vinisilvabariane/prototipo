namespace IrisWeb.Application.Communication.Auth.Requests;

public sealed record LoginRequest(uint Registration, string Password);
