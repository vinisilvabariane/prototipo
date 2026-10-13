using FluentResults;

namespace MeOne.Application.Errors;

public sealed class NotFoundError(string message) : Error(message);
