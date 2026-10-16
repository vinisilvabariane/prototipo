using Microsoft.AspNetCore.Authorization;

namespace MeOne.API.Auth;

public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;
