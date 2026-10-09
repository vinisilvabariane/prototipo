namespace IrisWeb.Application.Interfaces.ModelCatalog;

public interface IModelArtifactStorage
{

    Task SaveAsync(string objectKey, Stream content, CancellationToken cancellationToken);

    Task DeleteAsync(string objectKey, CancellationToken cancellationToken);

    Task<Stream?> OpenReadAsync(string objectKey, CancellationToken cancellationToken);

    Task<StoredObjectMetadata?> GetMetadataAsync(string objectKey, CancellationToken cancellationToken);
}

public sealed record StoredObjectMetadata(long Size, string ETag);
