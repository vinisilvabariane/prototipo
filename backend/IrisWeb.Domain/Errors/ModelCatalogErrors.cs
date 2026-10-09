namespace IrisWeb.Domain.Errors;

public static class ModelCatalogErrors
{
    public const string ModelNotFound = "Modelo não encontrado.";
    public const string VersionNotFound = "Versão não encontrada.";
    public const string KeyAlreadyExists = "Já existe um modelo com essa chave.";
    public const string VersionAlreadyExists = "Essa versão já existe para o modelo.";
    public const string UploadNotStarted = "O upload ainda não foi iniciado.";
    public const string ArtifactNotInStorage = "O artefato não foi encontrado no storage.";
    public const string ArtifactSizeMismatch = "O tamanho informado não corresponde ao artefato armazenado.";
    public const string NotApproved = "Somente versões aprovadas podem ser distribuídas.";
    public const string ArtifactMissing = "A versão não possui artefato.";
    public const string NothingPersisted = "Nenhuma alteração foi persistida no banco de dados.";
    public const string CannotArchiveWhileUploading = "Uma versão em upload não pode ser arquivada.";
    public const string ActorUnknown = "Não foi possível identificar a matrícula do usuário no token.";
    public const string CannotArchiveActiveVersion = "A versão ativa não pode ser arquivada. Ative outra versão antes.";
    public const string CannotDeleteActiveVersion = "A versão ativa não pode ser excluída. Ative outra versão antes.";
    public const string VersionBelongsToAnotherModel = "A versão pertence a outro modelo.";

    public static string ActiveVersionMustBeApproved(string actual) =>
        $"Só uma versão aprovada pode ser ativada; esta está em {actual}.";

    public static string InvalidTransition(string expected, string actual) =>
        $"A operação exige o estado {expected}, mas a versão está em {actual}.";

    public static string RuntimeMismatch(string platform, string required, string actual) =>
        $"Modelo {platform} exige runtime \"{required}\", mas a versão declarou \"{actual}\".";

    public static string ManifestFormatMismatch(string platform, string required, string actual) =>
        $"Modelo {platform} exige formato \"{required}\", mas o manifesto declarou \"{actual}\".";

    public static string EntrypointMismatch(string platform, string requiredExtension, string actual) =>
        $"Modelo {platform} exige entrypoint terminado em \"{requiredExtension}\", mas o manifesto declarou \"{actual}\".";
}
