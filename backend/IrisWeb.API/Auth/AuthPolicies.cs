using IrisWeb.Domain.Rules;

namespace IrisWeb.API.Auth;

public static class AuthPolicies
{
    public const string MasterRole = AccessRoles.Master;

    public const string MasterOnly = "MasterOnly";

    /// <summary>
    /// Limite por IP nas rotas anônimas que conferem ou trocam senha (login, valid, reset,
    /// autocadastro, consulta de colaborador) — freia força bruta na matrícula.
    /// </summary>
    public const string AnonymousRateLimit = "AnonymousCredentials";
}
