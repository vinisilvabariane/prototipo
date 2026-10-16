namespace MeOne.Domain.Rules;

public sealed record PermissionDefinition(string Code, string Description, bool GlobalOnly);

public static class Permissions
{
    public const string UsersRead = "users.read";
    public const string UsersManage = "users.manage";
    public const string UsersResetPassword = "users.reset-password";
    public const string RolesRead = "roles.read";
    public const string RolesManage = "roles.manage";
    public const string RolesAssign = "roles.assign";
    public const string DepartmentsManage = "departments.manage";
    public const string ScopesManage = "scopes.manage";
    public const string SystemHealth = "system.health";

    public const string ProductionRead = "production.read";
    public const string ProductionExecute = "production.execute";
    public const string ProductionSupervise = "production.supervise";
    public const string QualityInspect = "quality.inspect";
    public const string QualityRelease = "quality.release";
    public const string MaintenanceRequest = "maintenance.request";
    public const string MaintenanceExecute = "maintenance.execute";
    public const string EngineeringManage = "engineering.manage";
    public const string PlanningManage = "planning.manage";
    public const string WorkflowUse = "workflow.use";
    public const string OrgChartManage = "orgchart.manage";
    public const string SuppliersRegister = "suppliers.register";
    public const string SuppliersManage = "suppliers.manage";

    public static IReadOnlyList<PermissionDefinition> All { get; } =
    [
        new(UsersRead, "Consultar usuários", GlobalOnly: true),
        new(UsersManage, "Cadastrar, editar e desativar usuários", GlobalOnly: true),
        new(UsersResetPassword, "Redefinir a senha de usuários", GlobalOnly: true),
        new(RolesRead, "Consultar papéis e permissões", GlobalOnly: true),
        new(RolesManage, "Criar e editar papéis e suas permissões", GlobalOnly: true),
        new(RolesAssign, "Atribuir e revogar papéis de usuários", GlobalOnly: true),
        new(DepartmentsManage, "Cadastrar e editar departamentos", GlobalOnly: true),
        new(ScopesManage, "Cadastrar e editar plantas, áreas, linhas e postos", GlobalOnly: true),
        new(SystemHealth, "Consultar a saúde detalhada do sistema", GlobalOnly: true),
        new(ProductionRead, "Consultar produção", GlobalOnly: false),
        new(ProductionExecute, "Apontar produção", GlobalOnly: false),
        new(ProductionSupervise, "Supervisionar produção (liberar, corrigir e encerrar apontamentos)", GlobalOnly: false),
        new(QualityInspect, "Registrar inspeções de qualidade", GlobalOnly: false),
        new(QualityRelease, "Liberar e bloquear material por qualidade", GlobalOnly: false),
        new(MaintenanceRequest, "Abrir chamados de manutenção", GlobalOnly: false),
        new(MaintenanceExecute, "Atender e encerrar chamados de manutenção", GlobalOnly: false),
        new(EngineeringManage, "Manter roteiros, setups, parâmetros de processo, instruções de trabalho, layouts de linha e cadastros de produção", GlobalOnly: false),
        new(PlanningManage, "Planejar e sequenciar ordens de produção", GlobalOnly: false),
        new(WorkflowUse, "Criar, acompanhar e decidir workflows de aprovação", GlobalOnly: false),
        new(OrgChartManage, "Cadastrar e reorganizar os cargos do organograma", GlobalOnly: true),
        new(SuppliersRegister, "Cadastrar fornecedores e manter os que cadastrou", GlobalOnly: false),
        new(SuppliersManage, "Alterar qualquer fornecedor", GlobalOnly: false),
    ];

    private static readonly Dictionary<string, PermissionDefinition> ByCode =
        All.ToDictionary(permission => permission.Code, StringComparer.Ordinal);

    public static PermissionDefinition? Find(string code) => ByCode.GetValueOrDefault(code);

    public static bool Exists(string code) => ByCode.ContainsKey(code);

    public static bool IsGlobalOnly(string code) => ByCode.TryGetValue(code, out var permission) && permission.GlobalOnly;
}
