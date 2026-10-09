using IrisWeb.Application.Interfaces.ModelCatalog;

namespace IrisWeb.Infrastructure.Storage;

public sealed class FileSystemModelArtifactStorage : IModelArtifactStorage
{

    private const int CopyBufferSize = 81920;

    private readonly string _root;

    public FileSystemModelArtifactStorage(ModelStorageOptions options)
    {
        _root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(options.Path));

        Directory.CreateDirectory(_root);
    }

    public async Task SaveAsync(string objectKey, Stream content, CancellationToken cancellationToken)
    {
        var path = ResolveWithinRoot(objectKey);
        var temporary = $"{path}.{Guid.NewGuid():N}.part";

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            await using (var file = new FileStream(
                temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, CopyBufferSize, useAsync: true))
            {
                await content.CopyToAsync(file, CopyBufferSize, cancellationToken);
            }

            File.Move(temporary, path, overwrite: true);
        }
        catch (Exception exception)
        {

            TryDelete(temporary);

            if (IsInfrastructureFailure(exception))
                throw new ModelArtifactStorageException(StorageUnavailable, exception);

            throw;
        }
    }

    public Task DeleteAsync(string objectKey, CancellationToken cancellationToken)
    {
        var path = ResolveWithinRoot(objectKey);

        try
        {
            if (File.Exists(path))
                File.Delete(path);

            RemoveEmptyDirectories(Path.GetDirectoryName(path));

            return Task.CompletedTask;
        }
        catch (Exception exception) when (IsInfrastructureFailure(exception))
        {
            throw new ModelArtifactStorageException(StorageUnavailable, exception);
        }
    }

    private void RemoveEmptyDirectories(string? directory)
    {
        while (!string.IsNullOrEmpty(directory)
            && !string.Equals(directory, _root, StringComparison.Ordinal)
            && Directory.Exists(directory)
            && !Directory.EnumerateFileSystemEntries(directory).Any())
        {
            Directory.Delete(directory);
            directory = Path.GetDirectoryName(directory);
        }
    }

    public Task<Stream?> OpenReadAsync(string objectKey, CancellationToken cancellationToken)
    {
        var path = ResolveWithinRoot(objectKey);

        try
        {
            if (!File.Exists(path))
                return Task.FromResult<Stream?>(null);

            Stream content = new FileStream(
                path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete,
                CopyBufferSize, useAsync: true);

            return Task.FromResult<Stream?>(content);
        }
        catch (FileNotFoundException)
        {

            return Task.FromResult<Stream?>(null);
        }
        catch (Exception exception) when (IsInfrastructureFailure(exception))
        {
            throw new ModelArtifactStorageException(StorageUnavailable, exception);
        }
    }

    public Task<StoredObjectMetadata?> GetMetadataAsync(string objectKey, CancellationToken cancellationToken)
    {
        var path = ResolveWithinRoot(objectKey);

        try
        {
            var file = new FileInfo(path);

            if (!file.Exists)
                return Task.FromResult<StoredObjectMetadata?>(null);

            return Task.FromResult<StoredObjectMetadata?>(new StoredObjectMetadata(file.Length, ETagOf(file)));
        }
        catch (Exception exception) when (IsInfrastructureFailure(exception))
        {
            throw new ModelArtifactStorageException(StorageUnavailable, exception);
        }
    }

    private static string ETagOf(FileInfo file) =>
        $"{file.LastWriteTimeUtc.Ticks:x}-{file.Length:x}";

    private string ResolveWithinRoot(string objectKey)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
            throw new ArgumentException("A chave do artefato nao pode ser vazia.", nameof(objectKey));

        if (objectKey.Contains('\\', StringComparison.Ordinal))
            throw RejectedKey(objectKey);

        if (Path.IsPathRooted(objectKey))
            throw RejectedKey(objectKey);

        if (objectKey.Split('/').Any(segment => segment is ".." or "." or ""))
            throw RejectedKey(objectKey);

        var resolved = Path.GetFullPath(Path.Combine(_root, objectKey));

        if (!resolved.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw RejectedKey(objectKey);

        return resolved;
    }

    private static ArgumentException RejectedKey(string objectKey) =>
        new($"Chave de artefato invalida: '{objectKey}'. Ela vira caminho dentro da pasta de " +
            "artefatos e nao pode escapar dela.", nameof(objectKey));

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception exception) when (IsInfrastructureFailure(exception))
        {

        }
    }

    private static bool IsInfrastructureFailure(Exception exception) =>
        exception is IOException or UnauthorizedAccessException or NotSupportedException;

    private const string StorageUnavailable = "Storage de artefatos indisponivel.";
}
