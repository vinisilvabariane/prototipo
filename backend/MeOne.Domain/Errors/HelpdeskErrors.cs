namespace MeOne.Domain.Errors;

public static class HelpdeskErrors
{
    public const int MaxNameLength = 60;
    public const int MaxDescriptionLength = 200;
    public const int MaxBadges = 5;
    public const int MaxBadgeLength = 30;
    public const int MaxSummaryFields = 3;
    public const int MaxFields = 60;
    public const int MaxOptions = 30;
    public const int MaxLabelLength = 200;
    public const int MaxPlaceholderLength = 200;
    public const int MaxFieldIdLength = 50;
    public const int MaxTextLength = 500;
    public const int MaxTextAreaLength = 4000;
    public const int MaxReasonLength = 500;
    public const int MaxNoteLength = 1000;
    public const int MaxUrlLength = 500;
    public const int MaxPageSize = 100;
    public const int MaxDefects = 100_000;

    public const string SectorNotFound = "Setor não encontrado.";
    public const string TemplateNotFound = "Modelo de campo não encontrado.";
    public const string TicketNotFound = "Chamado não encontrado.";
    public const string NameRequired = "Informe o nome.";
    public const string ColorInvalid = "Use uma cor no formato #RRGGBB.";
    public const string IconInvalid = "Escolha um dos ícones da lista.";
    public const string UrlInvalid = "Informe um endereço http:// ou https:// válido.";
    public const string SectorInactive = "O setor está desativado e não recebe chamados.";
    public const string SectorIsExternal = "Este setor abre um endereço externo e não recebe chamados aqui.";
    public const string SectorHasNoForm = "Este setor ainda não tem formulário.";
    public const string SummaryFieldInvalid = "O resumo só pode usar campos de pergunta do formulário (sem localização, parada ou defeitos).";
    public const string FormTooLarge = "O formulário pode ter no máximo 60 campos.";
    public const string FieldIdInvalid = "O identificador do campo deve começar com letra minúscula e usar só letras minúsculas, números e _.";
    public const string FieldIdDuplicated = "Há dois campos com o mesmo identificador.";
    public const string FieldLabelRequired = "Todo campo precisa de um título.";
    public const string OptionsRequired = "Lista suspensa e cards precisam de ao menos uma opção.";
    public const string OptionsNotAllowed = "Só lista suspensa e cards têm opções.";
    public const string OptionDuplicated = "Há duas opções com o mesmo valor no mesmo campo.";
    public const string OptionInvalid = "Toda opção precisa de valor e de texto.";
    public const string DependencyInvalid = "Um campo condicional precisa depender de uma lista ou cards que vêm antes dele.";
    public const string DependencyValueInvalid = "Um campo condicional usa uma opção que não existe no campo de que depende.";
    public const string SpecialDuplicated = "Localização, parada de linha e defeitos podem aparecer uma única vez cada.";
    public const string SpecialNeedsLocation = "Parada de linha e defeitos precisam de um campo de localização no mesmo formulário.";
    public const string SpecialNotAllowedInResolution = "O formulário de resposta não aceita localização, parada de linha nem defeitos.";
    public const string RoleUnknown = "Um dos papéis de atendimento não existe.";
    public const string TemplateTypeInvalid = "Modelo de campo só pode ser texto, área de texto, número, lista suspensa ou cards.";
    public const string LineRequired = "Selecione a linha.";
    public const string LineNotFound = "A linha selecionada não existe ou está desativada.";
    public const string PostNotInLine = "O posto escolhido não faz parte da produção em andamento nessa linha.";
    public const string ResolutionRequired = "O setor exige o formulário de resposta para finalizar o chamado.";
    public const string TicketNotOpen = "Este chamado já foi assumido, recusado ou finalizado.";
    public const string TicketNotInProgress = "Só dá para finalizar um chamado que está em atendimento.";
    public const string OnlyAssigneeResolves = "Só quem assumiu o chamado pode finalizá-lo.";
    public const string CannotAttend = "Seu perfil não atende chamados deste setor.";
    public const string ReasonRequired = "Informe o motivo da recusa.";
    public const string PeriodInvalid = "O fim do período não pode ser anterior ao início.";
    public const string NothingPersisted = "Nenhuma alteração foi persistida no banco de dados.";
    public const string NoteTooLong = "A observação pode ter no máximo 1000 caracteres.";
    public const string ReasonTooLong = "O motivo pode ter no máximo 500 caracteres.";

    public static string NameTaken(string name) => $"Já existe um cadastro chamado \"{name}\".";

    public static string FieldRequired(string label) => $"Responda \"{label}\".";

    public static string AnswerInvalid(string label, string why) => $"\"{label}\": {why}";

    public static string FieldTooLong(string field, int max) => $"{field} pode ter no máximo {max} caracteres.";
}
