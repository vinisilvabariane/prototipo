using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace IrisWeb.Infrastructure.Repositories;

public sealed class ModelCatalogRepository(IrisWebDbContext context)
    : Repository<AiModel>(context), IModelCatalogRepository
{
    public Task<bool> ModelKeyExistsAsync(string key, CancellationToken cancellationToken) =>
        DbSet.AnyAsync(model => model.Key == key, cancellationToken);

    public Task<bool> VersionExistsAsync(Guid modelId, string version, CancellationToken cancellationToken) =>
        Context.Set<ModelVersion>()
            .AnyAsync(item => item.ModelId == modelId && item.Version == version, cancellationToken);

    public async Task<IReadOnlyList<AiModel>> ListWithVersionsAsync(CancellationToken cancellationToken) =>
        await DbSet
            .AsNoTracking()
            .Include(model => model.Versions)
            .OrderBy(model => model.Name)
            .ToListAsync(cancellationToken);

    public Task<AiModel?> GetWithVersionsAsync(Guid id, CancellationToken cancellationToken) =>
        DbSet.Include(model => model.Versions)
            .FirstOrDefaultAsync(model => model.Id == id, cancellationToken);

    public Task<ModelVersion?> GetVersionAsync(Guid modelId, Guid versionId, CancellationToken cancellationToken) =>
        Context.Set<ModelVersion>()
            .FirstOrDefaultAsync(item => item.ModelId == modelId && item.Id == versionId, cancellationToken);

    public async Task<IReadOnlyList<ModelVersion>> GetVersionsAsync(
        Guid modelId,
        CancellationToken cancellationToken) =>
        await Context.Set<ModelVersion>()
            .Where(item => item.ModelId == modelId)
            .ToListAsync(cancellationToken);

    public void AddVersion(ModelVersion version) => Context.Set<ModelVersion>().Add(version);

    public void RemoveModelWithVersions(AiModel model, IEnumerable<ModelVersion> versions)
    {
        Context.Set<ModelVersion>().RemoveRange(versions);
        DbSet.Remove(model);
    }

    public void RemoveVersion(ModelVersion version) => Context.Set<ModelVersion>().Remove(version);
}
