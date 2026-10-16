using FluentResults;
using MeOne.Application.Communication.WorkInstructions.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.WorkInstructions;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.WorkInstructions;

public sealed class GetWorkInstructionUseCase(
    IWorkInstructionRepository instructions,
    IUserRepository users,
    ICurrentActor currentActor) : IGetWorkInstructionUseCase
{
    public async Task<Result<WorkInstructionDetailResponse>> ExecuteAsync(ulong id, CancellationToken cancellationToken)
    {
        var instruction = await instructions.GetByIdAsync(id, cancellationToken);
        if (instruction is null)
            return Result.Fail<WorkInstructionDetailResponse>(new NotFoundError(WorkInstructionErrors.NotFound));

        var people = (await users.GetByIdsIncludingInactiveAsync([instruction.CreatedBy, instruction.UpdatedBy], cancellationToken))
            .ToDictionary(user => user.Id);

        string NameOf(ulong userId) => people.TryGetValue(userId, out var user) ? user.Name : WorkflowOutcomeTexts.UnknownUser;

        var profile = await currentActor.GetAccessProfileAsync(cancellationToken);
        var canManage = profile?.Allows(Permissions.EngineeringManage) ?? false;

        return Result.Ok(WorkInstructionsMapping.ToDetail(
            instruction,
            NameOf(instruction.CreatedBy),
            NameOf(instruction.UpdatedBy),
            canManage));
    }
}
