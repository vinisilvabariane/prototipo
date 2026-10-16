using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Production.Requests;

public sealed record SaveLayoutDecorationRequest(long Id, DecorationKind Kind, int X, int Y, int Width, int Height, int Rotation, string? Label, bool Locked);
