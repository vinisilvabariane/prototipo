using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Tests.Fakes;

public sealed class FakeModelCatalogRepository
    : InMemoryRepository<AiModel, Guid>, IModelCatalogRepository
{
    private readonly List<ModelVersion> _versions = [];

    private readonly Dictionary<Guid, string> _tracked = [];

    private int _removedVersions;

    public IReadOnlyList<ModelVersion> Versions => _versions;

    public bool NadaPersiste { get; set; }

    public void SeedVersion(ModelVersion version) => _versions.Add(version);

    private static string Snapshot(ModelVersion version) => string.Join(
        '|',
        version.Status,
        version.ArtifactObjectKey,
        version.ArtifactSize,
        version.Sha256,
        version.ManifestJson,
        version.ApprovedBy,
        version.ApprovedAtUtc);

    private int PendingVersionWrites()
    {
        var pendentes = 0;

        foreach (var (id, anterior) in _tracked.ToArray())
        {
            var version = _versions.FirstOrDefault(item => item.Id == id);

            if (version is null)
                continue;

            var atual = Snapshot(version);

            if (atual == anterior)
                continue;

            _tracked[id] = atual;
            pendentes++;
        }

        return pendentes;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        var afetadas = await base.SaveChangesAsync(cancellationToken) + PendingVersionWrites() + _removedVersions;
        _removedVersions = 0;

        return NadaPersiste ? 0 : afetadas;
    }

    public Task<bool> ModelKeyExistsAsync(string key, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(item => item.Key == key));

    public Task<bool> VersionExistsAsync(Guid modelId, string version, CancellationToken cancellationToken) =>
        Task.FromResult(_versions.Any(item => item.ModelId == modelId && item.Version == version));

    public Task<IReadOnlyList<AiModel>> ListWithVersionsAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<AiModel>>([.. Items.OrderBy(item => item.Name)]);

    public Task<AiModel?> GetWithVersionsAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Items.FirstOrDefault(item => item.Id == id));

    public Task<ModelVersion?> GetVersionAsync(Guid modelId, Guid versionId, CancellationToken cancellationToken)
    {
        var version = _versions.FirstOrDefault(item => item.ModelId == modelId && item.Id == versionId);

        if (version is not null && !_tracked.ContainsKey(version.Id))
            _tracked[version.Id] = Snapshot(version);

        return Task.FromResult(version);
    }

    public Task<IReadOnlyList<ModelVersion>> GetVersionsAsync(Guid modelId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ModelVersion>>(
            [.. _versions.Where(item => item.ModelId == modelId)]);

    public void AddVersion(ModelVersion version)
    {
        _versions.Add(version);

        _tracked[version.Id] = string.Empty;
    }

    public void RemoveModelWithVersions(AiModel model, IEnumerable<ModelVersion> versions)
    {
        foreach (var version in versions)
        {
            _versions.Remove(version);
            _tracked.Remove(version.Id);
        }

        Delete(model);
    }

    public void RemoveVersion(ModelVersion version)
    {
        _versions.Remove(version);
        _tracked.Remove(version.Id);
        _removedVersions++;
    }
}
