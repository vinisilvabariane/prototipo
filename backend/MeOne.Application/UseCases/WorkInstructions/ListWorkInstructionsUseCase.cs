using FluentResults;
using MeOne.Application.Communication.WorkInstructions.Requests;
using MeOne.Application.Communication.WorkInstructions.Responses;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.WorkInstructions;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.WorkInstructions;

public sealed class ListWorkInstructionsUseCase(IWorkInstructionRepository instructions, ICurrentActor currentActor)
    : IListWorkInstructionsUseCase
{
    public async Task<Result<WorkInstructionPageResponse>> ExecuteAsync(
        WorkInstructionsQuery query,
        CancellationToken cancellationToken)
    {
        var search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();

        var total = await instructions.CountAsync(search, cancellationToken);
        var page = await instructions.SearchAsync(search, query.Page * query.PageSize, query.PageSize, cancellationToken);

        var profile = await currentActor.GetAccessProfileAsync(cancellationToken);

        return Result.Ok(new WorkInstructionPageResponse(
            [.. page.Select(WorkInstructionsMapping.ToSummary)],
            total,
            query.Page,
            query.PageSize,
            profile?.Allows(Permissions.EngineeringManage) ?? false));
    }
}
