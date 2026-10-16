using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Production.Responses;

public sealed record RunDecorationResponse(DecorationKind Kind, int X, int Y, int Width, int Height, int Rotation, string? Label);
