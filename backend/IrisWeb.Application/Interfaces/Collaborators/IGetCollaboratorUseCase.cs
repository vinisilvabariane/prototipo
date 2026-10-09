using FluentResults;
using IrisWeb.Application.Communication.Collaborators.Responses;

namespace IrisWeb.Application.Interfaces.Collaborators;

public interface IGetCollaboratorUseCase
{
    Task<Result<CollaboratorResponse>> ExecuteAsync(uint registration, CancellationToken cancellationToken);
}
