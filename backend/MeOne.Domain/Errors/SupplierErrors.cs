namespace MeOne.Domain.Errors;

public static class SupplierErrors
{
    public const int MaxSapCodeLength = 30;
    public const int MaxNameLength = 150;
    public const int MaxBranchLength = 100;
    public const int MaxTaxIdLength = 30;
    public const int MaxCorporateNameLength = 200;
    public const int MaxZipCodeLength = 20;
    public const int MaxDistrictLength = 100;
    public const int MaxStreetLength = 200;
    public const int MaxNumberLength = 20;
    public const int MaxContactLength = 100;
    public const int MaxPhoneLength = 30;
    public const int MaxEmailLength = 100;
    public const int MaxToolNameLength = 100;
    public const int MaxAssetTagLength = 50;
    public const int MaxSkuLength = 50;
    public const int MaxBusinessUnitLength = 50;
    public const int MaxMaterialTypeLength = 50;
    public const int MaxLinkLength = 500;
    public const int MaxDocumentCodeLength = 50;
    public const int MaxVersionLength = 20;
    public const int MaxFileNameLength = 255;
    public const int MaxRevision = 999;
    public const long MaxFileBytes = 10 * 1024 * 1024;

    public const string NotFound = "Fornecedor não encontrado.";
    public const string ToolNotFound = "Ferramental não encontrado.";
    public const string MaterialNotFound = "Material não encontrado.";
    public const string SelfAssessmentNotFound = "Auto avaliação não encontrada.";
    public const string AuditNotFound = "Auditoria não encontrada.";
    public const string DocumentNotFound = "Documento não encontrado.";
    public const string FileNotFound = "Arquivo não encontrado.";
    public const string ResponsibleNotFound = "O responsável não existe ou está desativado.";

    public const string SapCodeRequired = "Informe o código SAP do fornecedor.";
    public const string NameRequired = "Informe o nome fantasia do fornecedor.";
    public const string BranchRequired = "Informe a filial do fornecedor.";
    public const string OriginInvalid = "Selecione a origem do fornecedor.";
    public const string MultiBranchInvalid = "Selecione de qual filial Multi o fornecedor é.";
    public const string CriticalityInvalid = "Selecione a criticidade.";
    public const string ZipCodeRequired = "Informe o CEP.";
    public const string DistrictRequired = "Informe o bairro.";
    public const string CnpjInvalid = "O CNPJ informado não é válido.";
    public const string EmailInvalid = "Informe um e-mail válido.";
    public const string ToolNameRequired = "Informe o ferramental.";
    public const string SkuRequired = "Informe o SKU do material.";
    public const string ResponsibleRequired = "Selecione o responsável pela validação.";
    public const string DateRequired = "Informe a data.";
    public const string StatusInvalid = "Selecione o status do fornecedor.";
    public const string ScoreInvalid = "A pontuação precisa estar entre 0 e 100.";
    public const string RevisionInvalid = "O número da revisão precisa estar entre 0 e 999.";
    public const string LinkInvalid = "O link precisa ser um endereço http ou https válido.";
    public const string ValidityInvalid = "O tempo de vencimento da auditoria precisa ser 3, 6 ou 12 meses.";
    public const string DocumentKindInvalid = "Tipo de documento inválido.";
    public const string DocumentOwnerNotAllowed = "Este tipo de documento não pertence a um item.";
    public const string DocumentOwnerRequired = "Informe o item ao qual o documento pertence.";
    public const string FileRequired = "Anexe o arquivo.";
    public const string FileEmpty = "Não é possível enviar um arquivo vazio.";
    public const string FileInvalid = "O arquivo precisa ser PDF, JPG ou PNG.";
    public const string FileTooLarge = "O arquivo passa do tamanho máximo de 10 MB.";
    public const string OnlyOwnerOrManagerEdits =
        "Somente quem cadastrou o fornecedor, ou quem gerencia fornecedores, pode alterá-lo.";
    public const string NothingPersisted = "Nenhuma alteração foi persistida no banco de dados.";

    public static string SapCodeTaken(string code, string multiBranch) =>
        $"O código SAP \"{code}\" já está cadastrado para a filial {multiBranch}.";

    public static string FieldTooLong(string field, int max) =>
        $"{field} não pode passar de {max} caracteres.";
}
