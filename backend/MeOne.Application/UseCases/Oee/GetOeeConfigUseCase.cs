using FluentResults;
using MeOne.Application.Communication.Oee.Responses;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Oee;
using MeOne.Application.UseCases.Production;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Oee;

public sealed class GetOeeConfigUseCase(
    IOeeFormulaSetRepository sets,
    IUserRepository users,
    IOeeLossSource lossSource,
    ICurrentActor currentActor) : IGetOeeConfigUseCase
{
    private static readonly OeeFunctionResponse[] Functions =
    [
        new("min", "min(a, b, ...)", "Menor valor da lista."),
        new("max", "max(a, b, ...)", "Maior valor da lista."),
        new("abs", "abs(x)", "Valor absoluto."),
        new("clamp", "clamp(x, mínimo, máximo)", "Mantém x entre o mínimo e o máximo."),
        new("if", "if(condição, então, senão)", "Se a condição for verdadeira (≠ 0), devolve o então; senão, o senão. Compare com <, >, <=, >=, ==, !=."),
        new("round", "round(x, casas)", "Arredonda x para o número de casas."),
    ];

    public async Task<Result<OeeConfigResponse>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var list = await sets.ListAsync(cancellationToken);
        var names = await ProductionLookups.UsersAsync(users, list.Select(set => set.UpdatedBy), cancellationToken);
        var profile = await currentActor.GetAccessProfileAsync(cancellationToken);
        var losses = await lossSource.GetAsync(0, DateTime.UtcNow, DateTime.UtcNow, cancellationToken);

        return Result.Ok(new OeeConfigResponse(
            [.. list.Select(set => OeeMapping.ToResponse(set, names))],
            list.FirstOrDefault(set => set.IsActive)?.Id,
            OeeMapping.ToResponse(OeeFormulas.Default),
            [.. OeeCatalog.Variables.Select(variable => new OeeVariableResponse(
                variable.Key,
                variable.Label,
                variable.Description,
                variable.Unit,
                OeeMapping.SlotsOf(variable)))],
            [.. OeeFormulas.Slots.Select(slot => new OeeSlotResponse(slot.Key, slot.Label, OeeMapping.SlotGroup(slot.Scope)))],
            Functions,
            losses.Available,
            profile?.Allows(Permissions.EngineeringManage) ?? false));
    }
}
