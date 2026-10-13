namespace MeOne.Application.Communication.Auth.Requests;

public sealed record ValidateCredentialsRequest(uint Registration, string Password);
