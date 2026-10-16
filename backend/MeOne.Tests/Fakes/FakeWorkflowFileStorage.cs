using MeOne.Application.Interfaces.Workflows;

namespace MeOne.Tests.Fakes;

public sealed class FakeWorkflowFileStorage : IWorkflowFileStorage
{
    private readonly Dictionary<string, byte[]> _files = [];

    public IReadOnlyCollection<string> StoredNames => _files.Keys;

    public List<string> Deleted { get; } = [];

    public async Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);

        var storedName = string.IsNullOrEmpty(extension) ? $"file-{_files.Count + 1}" : $"file-{_files.Count + 1}.{extension}";
        _files[storedName] = buffer.ToArray();

        return storedName;
    }

    public Task<Stream?> OpenReadAsync(string storedName, CancellationToken cancellationToken) =>
        Task.FromResult<Stream?>(_files.TryGetValue(storedName, out var bytes) ? new MemoryStream(bytes) : null);

    public Task DeleteAsync(string storedName, CancellationToken cancellationToken)
    {
        Deleted.Add(storedName);
        _files.Remove(storedName);
        return Task.CompletedTask;
    }
}
