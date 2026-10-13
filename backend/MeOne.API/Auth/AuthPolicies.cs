using MeOne.Domain.Rules;

namespace MeOne.API.Auth;

public static class AuthPolicies
{
    public const string MasterRole = AccessRoles.Master;

    public const string MasterOnly = "MasterOnly";

    public const string AnonymousRateLimit = "AnonymousCredentials";
}
