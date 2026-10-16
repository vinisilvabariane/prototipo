namespace MeOne.Application.Communication.Auth.Responses;

public sealed record PermissionGrantResponse(string Permission, ulong? ScopeId);
