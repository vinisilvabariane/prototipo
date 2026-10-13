using System.Reflection;
using MeOne.API.Auth;
using MeOne.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace MeOne.Tests.API;

public sealed class AccessEndpointAuthorizationTests
{
    private static readonly Type[] AccessControllers =
    [
        typeof(AuthController),
        typeof(UsersController),
        typeof(RolesController),
        typeof(DepartmentsController),
    ];

    private static IEnumerable<MethodInfo> Actions(Type controller) => controller
        .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
        .Where(method => method.GetCustomAttributes<HttpMethodAttribute>().Any());

    private static string Name(MethodInfo action) => $"{action.DeclaringType!.Name}.{action.Name}";

    [Fact]
    public void EveryAction_DeclaresAccessExplicitly()
    {
        var withoutAttribute = AccessControllers
            .SelectMany(Actions)
            .Where(action =>
                action.GetCustomAttribute<AuthorizeAttribute>() is null &&
                action.GetCustomAttribute<AllowAnonymousAttribute>() is null)
            .Select(Name)
            .ToArray();

        Assert.True(withoutAttribute.Length == 0, $"Missing [Authorize]/[AllowAnonymous]: {string.Join(", ", withoutAttribute)}");
    }

    [Fact]
    public void AnonymousActions_AreOnlyTheLoginScreenOnes()
    {
        string[] expected =
        [
            "AuthController.Login",
            "AuthController.Refresh",
            "AuthController.Valid",
            "UsersController.ResetPassword",
            "UsersController.GetByRegistration",
            "UsersController.Register",
            "DepartmentsController.Names",
            "DepartmentsController.All",
        ];

        var anonymousActions = AccessControllers
            .SelectMany(Actions)
            .Where(action => action.GetCustomAttribute<AllowAnonymousAttribute>() is not null)
            .Select(Name)
            .Order()
            .ToArray();

        Assert.Equal(expected.Order(), anonymousActions);
    }

    [Fact]
    public void EveryUserRoleAndDepartmentWrite_RequiresMaster()
    {
        Type[] governance = [typeof(UsersController), typeof(RolesController), typeof(DepartmentsController)];

        var openWrites = governance
            .SelectMany(Actions)
            .Where(action => action.GetCustomAttribute<HttpGetAttribute>() is null)
            .Where(action => action.GetCustomAttribute<AllowAnonymousAttribute>() is null)
            .Where(action => action.GetCustomAttribute<AuthorizeAttribute>()?.Policy != AuthPolicies.MasterOnly)
            .Select(Name)
            .ToArray();

        Assert.True(openWrites.Length == 0, $"Write without MasterOnly: {string.Join(", ", openWrites)}");
    }
}
