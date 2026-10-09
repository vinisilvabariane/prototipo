using System.Reflection;
using IrisWeb.API.Auth;
using IrisWeb.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace IrisWeb.Tests.API;

/// <summary>
/// Não há política de fallback: endpoint sem atributo é anônimo. Esta trava amarra o mapa de acesso
/// do controle de acesso — qualquer mudança aqui tem que ser de propósito.
/// </summary>
public sealed class AccessEndpointAuthorizationTests
{
    private static readonly Type[] AccessControllers =
    [
        typeof(AuthController),
        typeof(UsersController),
        typeof(RolesController),
        typeof(DepartmentsController),
        typeof(CollaboratorsController),
    ];

    private static IEnumerable<MethodInfo> Actions(Type controller) => controller
        .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
        .Where(method => method.GetCustomAttributes<HttpMethodAttribute>().Any());

    private static string Name(MethodInfo action) => $"{action.DeclaringType!.Name}.{action.Name}";

    [Fact]
    public void Toda_action_declara_o_acesso_explicitamente()
    {
        var semAtributo = AccessControllers
            .SelectMany(Actions)
            .Where(action =>
                action.GetCustomAttribute<AuthorizeAttribute>() is null &&
                action.GetCustomAttribute<AllowAnonymousAttribute>() is null)
            .Select(Name)
            .ToArray();

        Assert.True(semAtributo.Length == 0, $"Sem [Authorize]/[AllowAnonymous]: {string.Join(", ", semAtributo)}");
    }

    [Fact]
    public void Anonimas_sao_so_as_da_tela_de_login()
    {
        string[] esperadas =
        [
            "AuthController.Login",
            "AuthController.Refresh",
            "AuthController.Valid",
            "UsersController.ResetPassword",
            "UsersController.GetByRegistration",
            "UsersController.Register",
            "DepartmentsController.Names",
            "DepartmentsController.All",
            "CollaboratorsController.GetByRegistration",
        ];

        var anonimas = AccessControllers
            .SelectMany(Actions)
            .Where(action => action.GetCustomAttribute<AllowAnonymousAttribute>() is not null)
            .Select(Name)
            .Order()
            .ToArray();

        Assert.Equal(esperadas.Order(), anonimas);
    }

    [Fact]
    public void Toda_escrita_de_usuario_role_e_departamento_exige_master()
    {
        Type[] governanca = [typeof(UsersController), typeof(RolesController), typeof(DepartmentsController)];

        var abertas = governanca
            .SelectMany(Actions)
            .Where(action => action.GetCustomAttribute<HttpGetAttribute>() is null)
            .Where(action => action.GetCustomAttribute<AllowAnonymousAttribute>() is null)
            .Where(action => action.GetCustomAttribute<AuthorizeAttribute>()?.Policy != AuthPolicies.MasterOnly)
            .Select(Name)
            .ToArray();

        Assert.True(abertas.Length == 0, $"Escrita sem MasterOnly: {string.Join(", ", abertas)}");
    }
}
