using FluentResults;
using MeOne.Application.Interfaces.Oee;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Oee;

public sealed class DeleteOeeFormulaSetUseCase(IOeeFormulaSetRepository sets) : IDeleteOeeFormulaSetUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, CancellationToken cancellationToken)
    {
        var set = await OeeSupport.OpenSetAsync(sets, id, cancellationToken);
        if (set.IsFailed)
            return set.ToResult();

        if (set.Value.IsActive)
            return Result.Fail(OeeErrors.ActiveSetCannotBeDeleted);

        sets.Delete(set.Value);

        return await sets.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(OeeErrors.NothingPersisted);
    }
}
