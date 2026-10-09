using IrisWeb.Domain.Entities;

namespace IrisWeb.Domain.Interfaces;

public interface IModelCatalogRepository : IRepository<AiModel>
{
    Task<bool> ModelKeyExistsAsync(string key, CancellationToken cancellationToken);

    Task<bool> VersionExistsAsync(Guid modelId, string version, CancellationToken cancellationToken);

    Task<IReadOnlyList<AiModel>> ListWithVersionsAsync(CancellationToken cancellationToken);

    Task<AiModel?> GetWithVersionsAsync(Guid id, CancellationToken cancellationToken);

    Task<ModelVersion?> GetVersionAsync(Guid modelId, Guid versionId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ModelVersion>> GetVersionsAsync(Guid modelId, CancellationToken cancellationToken);

    void AddVersion(ModelVersion version);

    void RemoveModelWithVersions(AiModel model, IEnumerable<ModelVersion> versions);

    void RemoveVersion(ModelVersion version);
}
