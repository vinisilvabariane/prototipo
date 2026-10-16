namespace MeOne.Domain.Rules;

public static class WorkflowOutcomeTexts
{
    public const string UnknownUser = "Usuário não identificado";

    public const string ApprovalRecorded = "Sua aprovação foi registrada. Aguardando os demais responsáveis.";
    public const string WorkflowApproved = "Workflow aprovado por todos os responsáveis.";
    public const string RejectionRecorded = "Sua rejeição foi registrada e aguarda a análise do criador.";
    public const string WorkflowRejected = "Workflow rejeitado com sucesso.";
    public const string AssigneeReplaced = "Responsável substituído com sucesso.";
    public const string AssigneeReplacedAndApproved =
        "Responsável substituído e workflow aprovado automaticamente, pois todos os demais já haviam aprovado.";

    public static string AssigneeAddedDescription(string person) => $"Responsável adicionado: {person}";

    public static string AssigneeReplacedDescription(string previous, string current) =>
        $"Substituição de responsável: {previous} → {current}";
}
