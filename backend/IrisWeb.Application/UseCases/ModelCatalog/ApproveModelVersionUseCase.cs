using FluentResults;
using IrisWeb.Application.Communication.ModelCatalog.Responses;
using IrisWeb.Domain.Enums;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Application.Interfaces;
using IrisWeb.Application.Interfaces.ModelCatalog;

namespace IrisWeb.Application.UseCases.ModelCatalog;

public sealed class ApproveModelVersionUseCase(
    IModelCatalogRepository repository,
    ICurrentActor currentActor) : IApproveModelVersionUseCase
{
    public async Task<Result<CatalogModelVersionResponse>> ExecuteAsync(
        Guid modelId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        if (ModelCatalogMapping.Actor(currentActor) is not { } actor)
            return Result.Fail(ModelCatalogErrors.ActorUnknown);

        var version = await repository.GetVersionAsync(modelId, versionId, cancellationToken);
        if (version is null) return Result.Fail(ModelCatalogErrors.VersionNotFound);

        if (!version.CanApprove)
            return Result.Fail(ModelCatalogMapping.InvalidTransition(ModelVersionStatus .Candidate, version.Status));

        try
        {
            version.Approve(actor);

            if (await repository.SaveChangesAsync(cancellationToken) <= 0)
                return Result.Fail(ModelCatalogErrors.NothingPersisted);

            return Result.Ok(ModelCatalogMapping.ToResponse(version));
        }
        catch (DomainRuleException exception)
        {
            return Result.Fail(exception.Message);
        }
    }
}
