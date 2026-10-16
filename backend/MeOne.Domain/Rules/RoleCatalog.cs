namespace MeOne.Domain.Rules;

public sealed record SystemRoleDefinition(ulong Id, string Name, string Description, IReadOnlyList<string> Permissions);

public static class RoleCatalog
{
    public const ulong AdministratorId = 1;

    public static IReadOnlyList<SystemRoleDefinition> All { get; } =
    [
        new(AdministratorId, AccessRoles.Master, "Administrador: acesso total ao sistema", []),
        new(2, "gestor-acessos", "Gestor de acessos: cadastra usuários e atribui papéis",
        [
            Permissions.UsersRead,
            Permissions.UsersManage,
            Permissions.UsersResetPassword,
            Permissions.RolesRead,
            Permissions.RolesAssign,
            Permissions.DepartmentsManage,
        ]),
        new(3, "supervisor-producao", "Supervisor de produção",
        [
            Permissions.ProductionRead,
            Permissions.ProductionExecute,
            Permissions.ProductionSupervise,
            Permissions.QualityInspect,
            Permissions.MaintenanceRequest,
        ]),
        new(4, "operador", "Operador de produção",
        [
            Permissions.ProductionRead,
            Permissions.ProductionExecute,
            Permissions.MaintenanceRequest,
        ]),
        new(5, "qualidade", "Qualidade",
        [
            Permissions.ProductionRead,
            Permissions.QualityInspect,
            Permissions.QualityRelease,
        ]),
        new(6, "manutencao", "Manutenção",
        [
            Permissions.ProductionRead,
            Permissions.MaintenanceRequest,
            Permissions.MaintenanceExecute,
        ]),
        new(7, "engenharia-processos", "Engenharia de processos",
        [
            Permissions.ProductionRead,
            Permissions.EngineeringManage,
            Permissions.QualityInspect,
        ]),
        new(8, "pcp", "Planejamento e controle da produção",
        [
            Permissions.ProductionRead,
            Permissions.PlanningManage,
        ]),
        new(9, "workflow", "Workflow: cria, acompanha e decide fluxos de aprovação",
        [
            Permissions.WorkflowUse,
        ]),
        new(10, "organograma", "Organograma: mantém a estrutura hierárquica de cargos",
        [
            Permissions.OrgChartManage,
        ]),
        new(11, "fornecedores", "Fornecedores: cadastra e mantém os fornecedores que registrou",
        [
            Permissions.SuppliersRegister,
        ]),
        new(12, "gestor-fornecedores", "Gestor de fornecedores: altera qualquer fornecedor",
        [
            Permissions.SuppliersRegister,
            Permissions.SuppliersManage,
        ]),
    ];
}
