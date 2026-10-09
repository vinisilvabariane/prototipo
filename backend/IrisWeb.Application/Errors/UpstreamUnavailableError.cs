using FluentResults;

namespace IrisWeb.Application.Errors;

public sealed class UpstreamUnavailableError(string message) : Error(message);
