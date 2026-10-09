using IrisWeb.Application.Interfaces.ModelCatalog;

namespace IrisWeb.Tests.Fakes;

public sealed class FakeModelArtifactStorage : IModelArtifactStorage
{
    private readonly Dictionary<string, byte[]> _objects = [];

    public ModelArtifactStorageException? Falha { get; set; }

    public StoredObjectMetadata? Metadata { get; set; }

    public int SaveCalls { get; private set; }

    public int DeleteCalls { get; private set; }

    public int OpenReadCalls { get; private set; }

    public int MetadataCalls { get; private set; }

    public int TotalCalls => SaveCalls + DeleteCalls + OpenReadCalls + MetadataCalls;

    public static ModelArtifactStorageException Indisponivel(string mensagem = "Disco inalcancavel.") =>
        new(mensagem, new IOException(mensagem));

    public byte[]? Conteudo(string objectKey) =>
        _objects.TryGetValue(objectKey, out var bytes) ? bytes : null;

    public void Seed(string objectKey, byte[] content) => _objects[objectKey] = content;

    public async Task SaveAsync(string objectKey, Stream content, CancellationToken cancellationToken)
    {
        SaveCalls++;
        if (Falha is not null) throw Falha;

        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        _objects[objectKey] = buffer.ToArray();
    }

    public Task DeleteAsync(string objectKey, CancellationToken cancellationToken)
    {
        DeleteCalls++;
        if (Falha is not null) throw Falha;

        _objects.Remove(objectKey);
        return Task.CompletedTask;
    }

    public Task<Stream?> OpenReadAsync(string objectKey, CancellationToken cancellationToken)
    {
        OpenReadCalls++;
        if (Falha is not null) throw Falha;

        return Task.FromResult<Stream?>(
            _objects.TryGetValue(objectKey, out var bytes) ? new MemoryStream(bytes) : null);
    }

    public Task<StoredObjectMetadata?> GetMetadataAsync(string objectKey, CancellationToken cancellationToken)
    {
        MetadataCalls++;
        if (Falha is not null) throw Falha;
        return Task.FromResult(Metadata);
    }
}
