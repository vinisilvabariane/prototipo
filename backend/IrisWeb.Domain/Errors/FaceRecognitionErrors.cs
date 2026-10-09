namespace IrisWeb.Domain.Errors;

public static class FaceRecognitionErrors
{
    public const string EmptyEmbedding = "Embedding vazio.";
    public const string EmptyRegistration = "Matrícula vazia.";
    public const string NothingPersisted = "Nenhuma alteração foi persistida no banco de dados.";
    public const string TooManyEmbeddings = "Fotos demais num cadastro só.";
    public const string MixedEmbeddingDimensions = "As fotos do cadastro vieram com tamanhos de embedding diferentes.";
    public const string ConfirmationWithoutActor = "Não foi possível identificar quem confirmou o cadastro parecido.";

    public const string UnknownLabel = "desconhecido";

    public const string AmbiguousLabel = "indefinido";
}
