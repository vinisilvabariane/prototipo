using FluentResults;
using IrisWeb.Application.Communication.ModelCatalog.Responses;
using IrisWeb.Domain.Enums;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Application.Interfaces.ModelCatalog;

namespace IrisWeb.Application.UseCases.ModelCatalog;

public sealed class CreateDownloadUrlUseCase(IModelCatalogRepository repository) : ICreateDownloadUrlUseCase
{
    public async Task<Result<DownloadUrlResponse>> ExecuteAsync(
        Guid modelId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var version = await repository.GetVersionAsync(modelId, versionId, cancellationToken);
        if (version is null) return Result.Fail(ModelCatalogErrors.VersionNotFound);

        if (version.Status != ModelVersionStatus.Approved)
            return Result.Fail(ModelCatalogErrors.NotApproved);

        if (version.ArtifactObjectKey is null)
            return Result.Fail(ModelCatalogErrors.ArtifactMissing);

        var url = ModelCatalogMapping.ArtifactUrl(modelId, version.Id);

        return Result.Ok(new DownloadUrlResponse(version.Id, url, DateTime.UtcNow.AddMinutes(15)));
    }
}
