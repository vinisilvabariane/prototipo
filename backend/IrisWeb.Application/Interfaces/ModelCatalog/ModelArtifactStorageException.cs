namespace IrisWeb.Application.Interfaces.ModelCatalog;

public sealed class ModelArtifactStorageException(string message, Exception inner)
    : Exception(message, inner);
