using FluentResults;
using MeOne.Application.Communication.Oee.Requests;
using MeOne.Application.Communication.Oee.Responses;
using MeOne.Application.Interfaces.Oee;
using MeOne.Application.UseCases.Production;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Oee;

public sealed class ListOeeRunsUseCase(
    IProductionRunRepository runs,
    IProductionLineRepository lines,
    IProductionProductRepository products,
    IProductionShiftRepository shifts,
    IOeeFormulaSetRepository sets,
    IOeeLossSource lossSource) : IListOeeRunsUseCase
{
    public const int DefaultDays = 7;
    public const int MaxDays = 92;

    public async Task<Result<OeeComparisonResponse>> ExecuteAsync(OeeRunsQuery query, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var to = query.To is { } end ? EndOfDay(end) : now;
        var from = query.From is { } start ? DateTime.SpecifyKind(start.Date, DateTimeKind.Utc) : to.AddDays(-DefaultDays);

        if (to < from)
            return Result.Fail<OeeComparisonResponse>(OeeErrors.PeriodInvalid);

        if ((to - from).TotalDays > MaxDays)
            return Result.Fail<OeeComparisonResponse>(OeeErrors.PeriodTooLong);

        var found = await runs.SearchStartedAsync(query.Status, query.LineId, from, to, OeeErrors.MaxCompareRuns, cancellationToken);
        var (lineMap, productMap, shiftMap) = await ProductionLookups.AllAsync(lines, products, shifts, cancellationToken);
        var (calculator, setName) = await OeeSupport.ActiveCalculatorAsync(sets, cancellationToken);

        var items = new List<OeeRunSummaryResponse>();
        var lossAvailable = false;

        foreach (var run in found)
        {
            if (!lineMap.TryGetValue(run.LineId, out var line)
                || !productMap.TryGetValue(run.ProductId, out var product)
                || !shiftMap.TryGetValue(run.ShiftId, out var shift)
                || run.StartedAt is not { } started)
                continue;

            var losses = await lossSource.GetAsync(run.LineId, started, run.EndedAt ?? now, cancellationToken);
            lossAvailable |= losses.Available;

            items.Add(OeeMapping.ToSummary(run, line, product, shift, calculator.Calculate(run, now, losses)));
        }

        return Result.Ok(new OeeComparisonResponse(from, to, items, lossAvailable, setName));
    }

    private static DateTime EndOfDay(DateTime value) =>
        DateTime.SpecifyKind(value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
}
