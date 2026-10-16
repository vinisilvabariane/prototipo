using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Production.Responses;

public sealed record LayoutPostResponse(ulong Id, string Name, PostKind Kind, int X, int Y, int Width, int Height, int Rotation, bool IsCritical, decimal? TargetPerHour, string? Operation);
