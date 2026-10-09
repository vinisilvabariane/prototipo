using FluentResults;

namespace IrisWeb.Application.Errors;

public sealed class NotFoundError(string message) : Error(message);
