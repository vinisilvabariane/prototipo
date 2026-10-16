namespace MeOne.Infrastructure.Storage;

public abstract class LocalFileStorage(string path)
{
    private readonly string _root = System.IO.Path.GetFullPath(path);

    public async Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_root);

        var storedName = string.IsNullOrEmpty(extension)
            ? Guid.NewGuid().ToString("N")
            : $"{Guid.NewGuid():N}.{extension}";

        await using var file = new FileStream(
            Resolve(storedName),
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await content.CopyToAsync(file, cancellationToken);

        return storedName;
    }

    public Task<Stream?> OpenReadAsync(string storedName, CancellationToken cancellationToken)
    {
        var fullPath = Resolve(storedName);

        Stream? stream = File.Exists(fullPath)
            ? new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 81920, useAsync: true)
            : null;

        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storedName, CancellationToken cancellationToken)
    {
        var fullPath = Resolve(storedName);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    private string Resolve(string storedName)
    {
        var fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(_root, storedName));

        if (!fullPath.StartsWith(_root + System.IO.Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new InvalidOperationException("Stored file name escapes the storage root.");

        return fullPath;
    }
}
