using FluentResults;
using IrisWeb.Application.Communication.Collaborators.Responses;
using IrisWeb.Application.Interfaces.Collaborators;
using IrisWeb.Application.Interfaces.Matrix;

namespace IrisWeb.Application.UseCases.Collaborators;

public sealed class GetCollaboratorUseCase(ISyncMatrixClient sync) : IGetCollaboratorUseCase
{
    public async Task<Result<CollaboratorResponse>> ExecuteAsync(uint registration, CancellationToken cancellationToken)
    {
        var collaborator = await CollaboratorsMapping.LookupAsync(sync, registration, cancellationToken);

        return collaborator.IsFailed
            ? collaborator.ToResult()
            : Result.Ok(CollaboratorsMapping.ToResponse(collaborator.Value));
    }
}
