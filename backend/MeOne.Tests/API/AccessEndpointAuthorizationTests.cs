using System.Reflection;
using MeOne.API.Auth;
using MeOne.API.Controllers;
using MeOne.Domain.Rules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;

namespace MeOne.Tests.API;

public sealed class AccessEndpointAuthorizationTests
{
    private static readonly Type[] AccessControllers =
    [
        typeof(AuthController),
        typeof(UsersController),
        typeof(RolesController),
        typeof(PermissionsController),
        typeof(DepartmentsController),
        typeof(ScopesController),
        typeof(WorkflowsController),
        typeof(OrgChartController),
        typeof(WorkInstructionsController),
        typeof(SuppliersController),
        typeof(ProductionSetupController),
        typeof(ProductionLayoutsController),
        typeof(ProductionRunsController),
    ];

    private static IEnumerable<MethodInfo> Actions(Type controller) => controller
        .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
        .Where(method => method.GetCustomAttributes<HttpMethodAttribute>().Any());

    private static string Name(MethodInfo action) => $"{action.DeclaringType!.Name}.{action.Name}";

    private static string? PermissionOf(MethodInfo action) =>
        action.GetCustomAttribute<RequirePermissionAttribute>()?.Permission;

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

        Assert.True(withoutAttribute.Length == 0, $"Missing access attribute: {string.Join(", ", withoutAttribute)}");
    }

    [Fact]
    public void AnonymousActions_AreOnlyTheLoginScreenOnes()
    {
        string[] expected =
        [
            "AuthController.Login",
            "AuthController.Refresh",
            "UsersController.RequestPasswordReset",
            "UsersController.ConfirmPasswordReset",
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

    [Theory]
    [InlineData("UsersController.List", Permissions.UsersRead)]
    [InlineData("UsersController.GetById", Permissions.UsersRead)]
    [InlineData("UsersController.GetByRegistration", Permissions.UsersRead)]
    [InlineData("UsersController.Roles", Permissions.UsersRead)]
    [InlineData("UsersController.Create", Permissions.UsersManage)]
    [InlineData("UsersController.Update", Permissions.UsersManage)]
    [InlineData("UsersController.Delete", Permissions.UsersManage)]
    [InlineData("UsersController.ChangePassword", Permissions.UsersResetPassword)]
    [InlineData("UsersController.IssueTemporaryPassword", Permissions.UsersResetPassword)]
    [InlineData("RolesController.List", Permissions.RolesRead)]
    [InlineData("RolesController.All", Permissions.RolesRead)]
    [InlineData("RolesController.GetById", Permissions.RolesRead)]
    [InlineData("RolesController.Create", Permissions.RolesManage)]
    [InlineData("RolesController.Update", Permissions.RolesManage)]
    [InlineData("RolesController.SetPermissions", Permissions.RolesManage)]
    [InlineData("RolesController.Delete", Permissions.RolesManage)]
    [InlineData("RolesController.Assign", Permissions.RolesAssign)]
    [InlineData("RolesController.Revoke", Permissions.RolesAssign)]
    [InlineData("PermissionsController.List", Permissions.RolesRead)]
    [InlineData("DepartmentsController.Create", Permissions.DepartmentsManage)]
    [InlineData("DepartmentsController.Update", Permissions.DepartmentsManage)]
    [InlineData("DepartmentsController.Delete", Permissions.DepartmentsManage)]
    [InlineData("ScopesController.Create", Permissions.ScopesManage)]
    [InlineData("ScopesController.Update", Permissions.ScopesManage)]
    [InlineData("ScopesController.Delete", Permissions.ScopesManage)]
    [InlineData("WorkflowsController.List", Permissions.WorkflowUse)]
    [InlineData("WorkflowsController.Stats", Permissions.WorkflowUse)]
    [InlineData("WorkflowsController.Users", Permissions.WorkflowUse)]
    [InlineData("WorkflowsController.GetById", Permissions.WorkflowUse)]
    [InlineData("WorkflowsController.Create", Permissions.WorkflowUse)]
    [InlineData("WorkflowsController.Update", Permissions.WorkflowUse)]
    [InlineData("WorkflowsController.Approve", Permissions.WorkflowUse)]
    [InlineData("WorkflowsController.Reject", Permissions.WorkflowUse)]
    [InlineData("WorkflowsController.AcceptRejection", Permissions.WorkflowUse)]
    [InlineData("WorkflowsController.ReturnToAssignee", Permissions.WorkflowUse)]
    [InlineData("WorkflowsController.ReturnToCreator", Permissions.WorkflowUse)]
    [InlineData("WorkflowsController.Messages", Permissions.WorkflowUse)]
    [InlineData("WorkflowsController.AddAssignee", Permissions.WorkflowUse)]
    [InlineData("WorkflowsController.ReplaceAssignee", Permissions.WorkflowUse)]
    [InlineData("WorkflowsController.Attachment", Permissions.WorkflowUse)]
    [InlineData("OrgChartController.AssignableUsers", Permissions.OrgChartManage)]
    [InlineData("OrgChartController.Create", Permissions.OrgChartManage)]
    [InlineData("OrgChartController.Update", Permissions.OrgChartManage)]
    [InlineData("OrgChartController.AssignHolder", Permissions.OrgChartManage)]
    [InlineData("OrgChartController.Move", Permissions.OrgChartManage)]
    [InlineData("OrgChartController.Delete", Permissions.OrgChartManage)]
    [InlineData("WorkInstructionsController.NextCode", Permissions.EngineeringManage)]
    [InlineData("WorkInstructionsController.Create", Permissions.EngineeringManage)]
    [InlineData("WorkInstructionsController.Update", Permissions.EngineeringManage)]
    [InlineData("WorkInstructionsController.Delete", Permissions.EngineeringManage)]
    [InlineData("SuppliersController.Users", Permissions.SuppliersRegister)]
    [InlineData("SuppliersController.Create", Permissions.SuppliersRegister)]
    [InlineData("SuppliersController.Update", Permissions.SuppliersRegister)]
    [InlineData("SuppliersController.AddTool", Permissions.SuppliersRegister)]
    [InlineData("SuppliersController.UpdateTool", Permissions.SuppliersRegister)]
    [InlineData("SuppliersController.RemoveTool", Permissions.SuppliersRegister)]
    [InlineData("SuppliersController.AddMaterial", Permissions.SuppliersRegister)]
    [InlineData("SuppliersController.UpdateMaterial", Permissions.SuppliersRegister)]
    [InlineData("SuppliersController.RemoveMaterial", Permissions.SuppliersRegister)]
    [InlineData("SuppliersController.AddSelfAssessment", Permissions.SuppliersRegister)]
    [InlineData("SuppliersController.UpdateSelfAssessment", Permissions.SuppliersRegister)]
    [InlineData("SuppliersController.RemoveSelfAssessment", Permissions.SuppliersRegister)]
    [InlineData("SuppliersController.AddAudit", Permissions.SuppliersRegister)]
    [InlineData("SuppliersController.UpdateAudit", Permissions.SuppliersRegister)]
    [InlineData("SuppliersController.RemoveAudit", Permissions.SuppliersRegister)]
    [InlineData("SuppliersController.AddDocument", Permissions.SuppliersRegister)]
    [InlineData("SuppliersController.RemoveDocument", Permissions.SuppliersRegister)]
    [InlineData("ProductionSetupController.CreateLine", Permissions.EngineeringManage)]
    [InlineData("ProductionSetupController.UpdateLine", Permissions.EngineeringManage)]
    [InlineData("ProductionSetupController.CreateShift", Permissions.EngineeringManage)]
    [InlineData("ProductionSetupController.UpdateShift", Permissions.EngineeringManage)]
    [InlineData("ProductionSetupController.CreateProduct", Permissions.EngineeringManage)]
    [InlineData("ProductionSetupController.UpdateProduct", Permissions.EngineeringManage)]
    [InlineData("ProductionLayoutsController.Create", Permissions.EngineeringManage)]
    [InlineData("ProductionLayoutsController.Save", Permissions.EngineeringManage)]
    [InlineData("ProductionLayoutsController.Delete", Permissions.EngineeringManage)]
    [InlineData("ProductionLayoutsController.Copy", Permissions.EngineeringManage)]
    [InlineData("ProductionRunsController.Candidates", Permissions.ProductionSupervise)]
    [InlineData("ProductionRunsController.Create", Permissions.ProductionSupervise)]
    [InlineData("ProductionRunsController.Allocate", Permissions.ProductionSupervise)]
    [InlineData("ProductionRunsController.Release", Permissions.ProductionSupervise)]
    [InlineData("ProductionRunsController.ReleaseAll", Permissions.ProductionSupervise)]
    [InlineData("ProductionRunsController.CopyPrevious", Permissions.ProductionSupervise)]
    [InlineData("ProductionRunsController.Start", Permissions.ProductionSupervise)]
    [InlineData("ProductionRunsController.Close", Permissions.ProductionSupervise)]
    [InlineData("ProductionRunsController.Cancel", Permissions.ProductionSupervise)]
    [InlineData("ProductionRunsController.RecordOutput", Permissions.ProductionExecute)]
    public void Action_RequiresExpectedPermission(string action, string permission)
    {
        var method = AccessControllers.SelectMany(Actions).Single(candidate => Name(candidate) == action);

        Assert.Equal(permission, PermissionOf(method));
    }

    [Fact]
    public void EveryWriteOnUsersRolesDepartmentsAndScopes_RequiresAPermission()
    {
        Type[] governance =
        [
            typeof(UsersController),
            typeof(RolesController),
            typeof(DepartmentsController),
            typeof(ScopesController),
            typeof(OrgChartController),
            typeof(WorkInstructionsController),
            typeof(SuppliersController),
            typeof(ProductionSetupController),
            typeof(ProductionLayoutsController),
            typeof(ProductionRunsController),
        ];

        var open = governance
            .SelectMany(Actions)
            .Where(action => action.GetCustomAttribute<Microsoft.AspNetCore.Mvc.HttpGetAttribute>() is null)
            .Where(action => action.GetCustomAttribute<AllowAnonymousAttribute>() is null)
            .Where(action => PermissionOf(action) is null)
            .Select(Name)
            .ToArray();

        Assert.True(open.Length == 0, $"Write without permission: {string.Join(", ", open)}");
    }

    [Fact]
    public void EveryWorkflowAction_RequiresTheWorkflowPermission()
    {
        var other = Actions(typeof(WorkflowsController))
            .Where(action => PermissionOf(action) != Permissions.WorkflowUse)
            .Select(Name)
            .ToArray();

        Assert.True(other.Length == 0, $"Workflow action without the workflow permission: {string.Join(", ", other)}");
    }

    [Fact]
    public void EveryRequiredPermission_ExistsInTheCatalog()
    {
        var unknown = AccessControllers
            .SelectMany(Actions)
            .Select(PermissionOf)
            .OfType<string>()
            .Where(permission => !Permissions.Exists(permission))
            .ToArray();

        Assert.Empty(unknown);
    }
}
