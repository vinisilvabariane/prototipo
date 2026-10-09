using FluentResults;
using IrisWeb.Application.Communication.ModelCatalog.Responses;
using IrisWeb.Domain.Enums;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Application.Interfaces.ModelCatalog;

namespace IrisWeb.Application.UseCases.ModelCatalog;

public sealed class CreateUploadUrlUseCase(IModelCatalogRepository repository) : ICreateUploadUrlUseCase
{
    public async Task<Result<UploadUrlResponse>> ExecuteAsync(
        Guid modelId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var version = await repository.GetVersionAsync(modelId, versionId, cancellationToken);
        if (version is null) return Result.Fail(ModelCatalogErrors.VersionNotFound);

        if (!version.CanBeginUpload)
            return Result.Fail(ModelCatalogMapping.InvalidTransition(ModelVersionStatus.Draft, version.Status));

        var model = await repository.GetWithVersionsAsync(modelId, cancellationToken);
        if (model is null) return Result.Fail(ModelCatalogErrors.ModelNotFound);

        var objectKey = $"{model.Key}/{version.Version}/model-package.zip";

        try
        {
            version.BeginUpload(objectKey);

            if (await repository.SaveChangesAsync(cancellationToken) <= 0)
                return Result.Fail(ModelCatalogErrors.NothingPersisted);

            var url = ModelCatalogMapping.ArtifactUrl(modelId, version.Id);

            return Result.Ok(new UploadUrlResponse(version.Id, "PUT", url, DateTime.UtcNow.AddMinutes(15)));
        }
        catch (DomainRuleException exception)
        {
            return Result.Fail(exception.Message);
        }
    }
}
