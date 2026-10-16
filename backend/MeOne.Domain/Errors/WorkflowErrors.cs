namespace MeOne.Domain.Errors;

public static class WorkflowErrors
{
    public const int MaxTitleLength = 100;
    public const int MaxDescriptionLength = 500;
    public const int MaxStepDescriptionLength = 600;
    public const int MaxJustificationLength = 1000;
    public const int MaxAttachments = 5;
    public const long MaxAttachmentBytes = 10 * 1024 * 1024;
    public const int MaxAttachmentNameLength = 255;
    public const int MaxContentTypeLength = 150;

    public const string NotFound = "Workflow não encontrado.";
    public const string StepNotFound = "Etapa do workflow não encontrada.";
    public const string AttachmentNotFound = "Anexo não encontrado.";
    public const string AssigneeNotFound = "Responsável não encontrado para este workflow.";
    public const string UserNotFound = "Usuário não encontrado.";

    public const string TitleRequired = "Informe o título do workflow.";
    public const string FilterInvalid = "Filtro de workflows inválido.";
    public const string PriorityInvalid = "Selecione uma prioridade válida.";
    public const string CategoryInvalid = "Selecione uma categoria válida.";
    public const string DueDateRequired = "Informe o prazo do workflow.";
    public const string DueDateInPast = "A data do prazo final não pode ser menor que a data atual.";
    public const string JustificationRequired = "Informe uma justificativa.";
    public const string UserRequired = "Selecione um usuário.";
    public const string NewAssigneeRequired = "É necessário selecionar um novo responsável para substituir o removido.";
    public const string UnknownAssignees = "Um ou mais responsáveis informados não existem ou estão desativados.";
    public const string NewAssigneeUnknown = "O novo responsável não existe ou está desativado.";

    public const string TooManyAttachments = "Você pode enviar no máximo 5 arquivos.";
    public const string AttachmentEmpty = "Não é possível anexar um arquivo vazio.";
    public const string NothingPersisted =
        "Não foi possível salvar: nenhuma alteração foi persistida ou o workflow foi alterado por outra pessoa. Atualize a página e tente de novo.";

    public const string NotAssignee = "Você não é responsável por este workflow.";
    public const string OnlyCreatorEdits = "Somente o criador do workflow pode editá-lo.";
    public const string OnlyCreatorManagesAssignees = "Somente o criador do workflow pode alterar os responsáveis.";
    public const string OnlyCreatorReviews = "Somente o criador do workflow pode analisar uma rejeição.";
    public const string OnlyRejecterReviews = "Somente quem rejeitou o workflow pode revisar esta rejeição.";

    public const string AlreadyApproved = "Você já aprovou este workflow.";
    public const string RejectionUnderCreatorReview = "Sua rejeição está em análise pelo criador do workflow.";
    public const string RejectionAlreadyOpen = "Você já tem uma rejeição em andamento neste workflow.";
    public const string NoRejectionAwaitingCreator = "Não há nenhuma rejeição aguardando a análise do criador.";
    public const string RejectionNotAwaitingCreator = "Esta rejeição não está aguardando a análise do criador.";
    public const string RejectionNotAwaitingAssignee = "Esta rejeição não está aguardando a revisão de quem a fez.";
    public const string StepIsNotRejection = "A etapa informada não é uma rejeição.";

    public const string CreatorIsAlreadyAssignee = "O criador do workflow já é automaticamente um responsável.";
    public const string AlreadyAssignee = "Este usuário já é responsável por este workflow.";
    public const string CannotRemoveCreator = "Não é possível remover o criador do workflow.";
    public const string AssigneeAlreadyApproved = "Não é possível substituir um responsável que já aprovou o workflow.";
    public const string NewAssigneeAlreadyListed = "O novo responsável selecionado já está na lista de responsáveis.";

    public static string AlreadyFinished(string statusLabel) =>
        $"Este workflow já foi {statusLabel} e não aceita novas ações.";

    public static string AttachmentTooLarge(string fileName) =>
        $"O arquivo \"{fileName}\" excede o tamanho máximo de 10MB.";

    public static string AttachmentNameTooLong(string fileName) =>
        $"O nome do arquivo \"{fileName}\" passa de {MaxAttachmentNameLength} caracteres.";

    public static string FieldTooLong(string field, int max) =>
        $"{field} não pode passar de {max} caracteres.";
}
