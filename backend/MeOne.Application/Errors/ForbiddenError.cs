using FluentResults;

namespace MeOne.Application.Errors;

public sealed class ForbiddenError(string message) : Error(message);
