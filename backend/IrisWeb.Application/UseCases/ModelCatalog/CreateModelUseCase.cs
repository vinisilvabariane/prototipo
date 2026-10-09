using FluentResults;
using IrisWeb.Application.Communication.ModelCatalog.Requests;
using IrisWeb.Application.Communication.ModelCatalog.Responses;
using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Application.Interfaces;
using IrisWeb.Application.Interfaces.ModelCatalog;

namespace IrisWeb.Application.UseCases.ModelCatalog;

public sealed class CreateModelUseCase(
    IModelCatalogRepository repository,
    ICurrentActor currentActor) : ICreateModelUseCase
{
    public async Task<Result<CatalogModelResponse>> ExecuteAsync(
        CreateModelRequest request,
        CancellationToken cancellationToken)
    {
        if (ModelCatalogMapping.Actor(currentActor) is not { } actor)
            return Result.Fail(ModelCatalogErrors.ActorUnknown);

        var key = request.Key.Trim().ToLowerInvariant();
        if (await repository.ModelKeyExistsAsync(key, cancellationToken))
            return Result.Fail(ModelCatalogErrors.KeyAlreadyExists);

        var model = AiModel.Create(
            key, request.Name, request.Description, request.TaskType, request.Platform, actor);
        await repository.AddAsync(model, cancellationToken);

        if (await repository.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(ModelCatalogErrors.NothingPersisted);

        return Result.Ok(ModelCatalogMapping.ToResponse(model));
    }
}
