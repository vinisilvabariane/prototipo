using FluentResults;
using IrisWeb.Application.Communication.ModelCatalog.Requests;
using IrisWeb.Application.Communication.ModelCatalog.Responses;
using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Application.Interfaces;
using IrisWeb.Application.Interfaces.ModelCatalog;

namespace IrisWeb.Application.UseCases.ModelCatalog;

public sealed class CreateModelVersionUseCase(
    IModelCatalogRepository repository,
    ICurrentActor currentActor) : ICreateModelVersionUseCase
{
    public async Task<Result<CatalogModelVersionResponse>> ExecuteAsync(
        Guid modelId,
        CreateModelVersionRequest request,
        CancellationToken cancellationToken)
    {
        if (ModelCatalogMapping.Actor(currentActor) is not { } actor)
            return Result.Fail(ModelCatalogErrors.ActorUnknown);

        var model = await repository.GetWithVersionsAsync(modelId, cancellationToken);
        if (model is null)
            return Result.Fail(ModelCatalogErrors.ModelNotFound);

        if (ModelCatalogMapping.RuntimeViolation(model, request.Runtime) is { } violation)
            return Result.Fail(violation);

        if (await repository.VersionExistsAsync(modelId, request.Version, cancellationToken))
            return Result.Fail(ModelCatalogErrors.VersionAlreadyExists);

        var version = ModelVersion.Create(modelId, request.Version, request.Runtime, actor);
        repository.AddVersion(version);

        if (await repository.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(ModelCatalogErrors.NothingPersisted);

        return Result.Ok(ModelCatalogMapping.ToResponse(version));
    }
}
