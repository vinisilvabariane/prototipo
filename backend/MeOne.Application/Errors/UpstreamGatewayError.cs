using FluentResults;

namespace MeOne.Application.Errors;

public sealed class UpstreamGatewayError(string message) : Error(message);
