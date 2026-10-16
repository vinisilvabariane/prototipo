using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Production.Responses;

public sealed record LayoutDecorationResponse(ulong Id, DecorationKind Kind, int X, int Y, int Width, int Height, int Rotation, string? Label, bool Locked);
