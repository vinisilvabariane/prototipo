using FluentResults;

namespace IrisWeb.Application.Errors;

public sealed class CredentialRejectedError(string message) : Error(message);
