using FluentResults;

namespace IrisWeb.Application.Errors;

public sealed class UpstreamGatewayError(string message) : Error(message);
