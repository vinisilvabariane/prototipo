using FluentResults;

namespace MeOne.Application.Errors;

public sealed class UpstreamUnavailableError(string message) : Error(message);
