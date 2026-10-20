using FluentResults;
using MeOne.Application.Communication.Oee.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.Oee;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Oee;

public sealed class SetActiveOeeFormulaSetUseCase(IOeeFormulaSetRepository sets) : ISetActiveOeeFormulaSetUseCase
{
    public async Task<Result> ExecuteAsync(SetActiveOeeFormulaSetRequest request, CancellationToken cancellationToken)
    {
        var all = await sets.ListAsync(cancellationToken);

        if (request.SetId is { } id && all.All(set => set.Id != id))
            return Result.Fail(new NotFoundError(OeeErrors.SetNotFound));

        var changed = false;

        foreach (var set in all)
        {
            var shouldBeActive = set.Id == request.SetId;

            if (set.IsActive == shouldBeActive)
                continue;

            if (shouldBeActive)
                set.Activate();
            else
                set.Deactivate();

            sets.Update(set);
            changed = true;
        }

        if (!changed)
            return Result.Ok();

        return await sets.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(OeeErrors.NothingPersisted);
    }
}
