using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Scopes.Requests;

public sealed record CreateScopeRequest(ScopeType Type, string Code, string Name, ulong? ParentId);
