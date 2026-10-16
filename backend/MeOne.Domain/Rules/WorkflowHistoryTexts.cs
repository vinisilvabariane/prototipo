namespace MeOne.Domain.Rules;

public static class WorkflowHistoryTexts
{
    public const string Created = "Workflow criado pelo usuário";
    public const string CreatedNote = "Workflow criado";

    public const string CreatorAutoApproval = "Aprovação automática do criador";
    public const string CreatorAutoApprovalNote = "Aprovado automaticamente ao criar o workflow";

    public const string Edited = "Workflow editado";
    public const string EditedNote = "Processo editado pelo criador do workflow";

    public const string Approved = "Workflow aprovado pelo usuário";
    public const string Rejected = "Workflow rejeitado pelo usuário";

    public const string FinalApproval = "Workflow aprovado por todos os responsáveis";
    public const string FinalApprovalNote = "Aprovação final concluída";

    public const string FinalApprovalAfterReplacement = "Workflow aprovado por todos os responsáveis após substituição";
    public const string FinalApprovalAfterReplacementNote =
        "Aprovação final concluída automaticamente após substituição de responsável";

    public const string FinalRejection = "Workflow rejeitado pelo criador";

    public const string AssigneeReplacedNote = "Responsável substituído mediante solicitação do criador";
}
