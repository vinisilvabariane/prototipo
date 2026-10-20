using FluentResults;
using MeOne.Application.Communication.Oee.Responses;
using MeOne.Application.Interfaces.Oee;
using MeOne.Application.UseCases.Production;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Oee;

public sealed class GetRunOeeUseCase(
    IProductionRunRepository runs,
    IProductionLineRepository lines,
    IProductionProductRepository products,
    IProductionShiftRepository shifts,
    IOeeFormulaSetRepository sets,
    IOeeLossSource lossSource) : IGetRunOeeUseCase
{
    public async Task<Result<RunOeeResponse>> ExecuteAsync(ulong id, CancellationToken cancellationToken)
    {
        var run = await ProductionLookups.OpenRunAsync(runs, id, cancellationToken);
        if (run.IsFailed)
            return run.ToResult<RunOeeResponse>();

        var lookup = await ProductionLookups.FindAsync(lines, products, shifts, run.Value.LineId, run.Value.ProductId, run.Value.ShiftId, false, cancellationToken);
        if (lookup.IsFailed)
            return lookup.ToResult<RunOeeResponse>();

        var now = DateTime.UtcNow;
        var (calculator, setName) = await OeeSupport.ActiveCalculatorAsync(sets, cancellationToken);

        var losses = run.Value.StartedAt is { } started
            ? await lossSource.GetAsync(run.Value.LineId, started, run.Value.EndedAt ?? now, cancellationToken)
            : Domain.Rules.OeeLosses.None;

        var oee = calculator.Calculate(run.Value, now, losses);

        return Result.Ok(OeeMapping.ToResponse(run.Value, lookup.Value.Line, lookup.Value.Product, lookup.Value.Shift, oee, setName));
    }
}
