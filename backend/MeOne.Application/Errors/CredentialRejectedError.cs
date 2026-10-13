using FluentResults;

namespace MeOne.Application.Errors;

public sealed class CredentialRejectedError(string message) : Error(message);
