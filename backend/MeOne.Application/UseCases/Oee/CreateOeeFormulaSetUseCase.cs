using FluentResults;
using MeOne.Application.Communication.Oee.Requests;
using MeOne.Application.Communication.Oee.Responses;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Oee;
using MeOne.Application.UseCases.Production;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Oee;

public sealed class CreateOeeFormulaSetUseCase(
    IOeeFormulaSetRepository sets,
    IUserRepository users,
    ICurrentActor currentActor) : ICreateOeeFormulaSetUseCase
{
    public async Task<Result<OeeFormulaSetResponse>> ExecuteAsync(SaveOeeFormulaSetRequest request, CancellationToken cancellationToken)
    {
        var actor = await ProductionLookups.ActorIdAsync(currentActor, cancellationToken);
        if (actor.IsFailed)
            return actor.ToResult<OeeFormulaSetResponse>();

        var name = OeeFormulaSet.NormalizeName(request.Name!);

        if (await sets.NameExistsAsync(name, null, cancellationToken))
            return Result.Fail<OeeFormulaSetResponse>(OeeErrors.NameTaken(name));

        var created = OeeFormulaSet.Create(name, OeeMapping.ToDomain(request.Formulas!), actor.Value, DateTime.UtcNow);

        if (request.Activate)
        {
            foreach (var other in await sets.ListAsync(cancellationToken))
            {
                other.Deactivate();
                sets.Update(other);
            }

            created.Activate();
        }

        await sets.AddAsync(created, cancellationToken);

        if (await sets.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail<OeeFormulaSetResponse>(OeeErrors.NothingPersisted);

        var names = await ProductionLookups.UsersAsync(users, [actor.Value], cancellationToken);

        return Result.Ok(OeeMapping.ToResponse(created, names));
    }
}
