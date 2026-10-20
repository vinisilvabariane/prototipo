using FluentResults;
using MeOne.Application.Errors;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Oee;

internal static class OeeSupport
{
    public static async Task<(OeeCalculator Calculator, string Name)> ActiveCalculatorAsync(
        IOeeFormulaSetRepository sets,
        CancellationToken cancellationToken)
    {
        var active = await sets.GetActiveAsync(cancellationToken);

        return active is null
            ? (OeeCalculator.Default, OeeMapping.DefaultSetName)
            : (new OeeCalculator(active.Formulas), active.Name);
    }

    public static async Task<Result<OeeFormulaSet>> OpenSetAsync(
        IOeeFormulaSetRepository sets,
        ulong id,
        CancellationToken cancellationToken)
    {
        var set = await sets.GetByIdAsync(id, cancellationToken);

        return set is null
            ? Result.Fail<OeeFormulaSet>(new NotFoundError(OeeErrors.SetNotFound))
            : Result.Ok(set);
    }
}
