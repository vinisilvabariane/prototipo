using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Scopes.Responses;

public sealed record ScopeResponse(ulong Id, ScopeType Type, string Code, string Name, ulong? ParentId);
