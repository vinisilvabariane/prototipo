namespace MeOne.Domain.Errors;

public static class WorkInstructionErrors
{
    public const int MaxCodeLength = 30;
    public const int MaxTitleLength = 150;
    public const int MaxDocumentNumberLength = 50;
    public const int MaxProductLength = 100;
    public const int MaxPersonLength = 100;
    public const int MaxPpeLength = 1000;
    public const int MaxSpecificationsLength = 2000;
    public const int MaxNotesLength = 1000;
    public const int MaxStepTextLength = 2000;
    public const int MaxSteps = 50;
    public const int MaxFileNameLength = 255;
    public const long MaxPdfBytes = 10 * 1024 * 1024;
    public const long MaxImageBytes = 5 * 1024 * 1024;

    public const string NotFound = "Instrução de trabalho não encontrada.";
    public const string StepNotFound = "Passo não encontrado.";
    public const string FileNotFound = "Arquivo não encontrado.";
    public const string CodeRequired = "Informe o código da instrução.";
    public const string TitleRequired = "Informe o título da instrução.";
    public const string DocumentNumberRequired = "Informe o número do documento.";
    public const string ProductRequired = "Informe o produto.";
    public const string PreparedByRequired = "Informe quem elaborou a instrução.";
    public const string ApprovedByRequired = "Informe quem aprovou a instrução.";
    public const string PpeRequired = "Informe os EPIs obrigatórios.";
    public const string SpecificationsRequired = "Informe as especificações de processo.";
    public const string StepTextRequired = "Todo passo precisa de uma instrução escrita.";
    public const string PdfRequired = "Anexe o PDF da instrução.";
    public const string PdfInvalid = "O arquivo enviado não é um PDF válido.";
    public const string PdfTooLarge = "O PDF passa do tamanho máximo de 10 MB.";
    public const string ImageInvalid = "A imagem do passo precisa ser PNG, JPEG, GIF ou WebP.";
    public const string ImageTooLarge = "A imagem do passo passa do tamanho máximo de 5 MB.";
    public const string FileEmpty = "Não é possível enviar um arquivo vazio.";
    public const string NothingPersisted = "Nenhuma alteração foi persistida no banco de dados.";

    public static string TooManySteps => $"A instrução pode ter no máximo {MaxSteps} passos.";

    public static string CodeTaken(string code) => $"O código \"{code}\" já está em uso.";

    public static string FieldTooLong(string field, int max) =>
        $"{field} não pode passar de {max} caracteres.";
}
