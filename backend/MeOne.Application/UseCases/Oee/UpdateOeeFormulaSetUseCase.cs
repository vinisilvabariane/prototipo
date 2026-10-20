using FluentResults;
using MeOne.Application.Communication.Oee.Requests;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Oee;
using MeOne.Application.UseCases.Production;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Oee;

public sealed class UpdateOeeFormulaSetUseCase(IOeeFormulaSetRepository sets, ICurrentActor currentActor) : IUpdateOeeFormulaSetUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, SaveOeeFormulaSetRequest request, CancellationToken cancellationToken)
    {
        var set = await OeeSupport.OpenSetAsync(sets, id, cancellationToken);
        if (set.IsFailed)
            return set.ToResult();

        var actor = await ProductionLookups.ActorIdAsync(currentActor, cancellationToken);
        if (actor.IsFailed)
            return actor.ToResult();

        var name = OeeFormulaSet.NormalizeName(request.Name!);

        if (await sets.NameExistsAsync(name, id, cancellationToken))
            return Result.Fail(OeeErrors.NameTaken(name));

        var changed = set.Value.Update(name, OeeMapping.ToDomain(request.Formulas!), actor.Value, DateTime.UtcNow);

        if (request.Activate && !set.Value.IsActive)
        {
            foreach (var other in await sets.ListAsync(cancellationToken))
            {
                other.Deactivate();
                sets.Update(other);
            }

            set.Value.Activate();
            changed = true;
        }

        if (!changed)
            return Result.Ok();

        sets.Update(set.Value);

        return await sets.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(OeeErrors.NothingPersisted);
    }
}
