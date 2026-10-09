using FluentResults;

namespace IrisWeb.Application.Errors;

public sealed class ForbiddenError(string message) : Error(message);
