using FluentResults;
using MeOne.Application.Communication.Oee.Requests;
using MeOne.Application.Communication.Oee.Responses;
using MeOne.Application.Interfaces.Oee;
using MeOne.Application.UseCases.Production;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Oee;

public sealed class TestOeeFormulaUseCase(
    IProductionRunRepository runs,
    IOeeFormulaSetRepository sets,
    IOeeLossSource lossSource) : ITestOeeFormulaUseCase
{
    public async Task<Result<OeeFormulaTestResponse>> ExecuteAsync(TestOeeFormulaRequest request, CancellationToken cancellationToken)
    {
        var slot = OeeFormulas.Slots.First(candidate => candidate.Key == request.Slot);
        var allowed = OeeCatalog.AllowedIn(slot.Scope);

        var (expression, error) = OeeFormula.Parse(request.Expression);

        if (error is not null)
            return Result.Ok(Failed(error.Message, error.Position));

        var unknown = OeeFormula.Validate(request.Expression, allowed);

        if (unknown is not null)
            return Result.Ok(Failed(unknown.Message, unknown.Position, [.. expression!.Identifiers]));

        var values = new Dictionary<string, double>();

        if (request.RunId is { } runId)
        {
            var run = await ProductionLookups.OpenRunAsync(runs, runId, cancellationToken);
            if (run.IsFailed)
                return run.ToResult<OeeFormulaTestResponse>();

            if (run.Value.StartedAt is not { } started)
                return Result.Fail<OeeFormulaTestResponse>(OeeErrors.RunNotStarted);

            var now = DateTime.UtcNow;
            var losses = await lossSource.GetAsync(run.Value.LineId, started, run.Value.EndedAt ?? now, cancellationToken);
            var (calculator, _) = await OeeSupport.ActiveCalculatorAsync(sets, cancellationToken);
            var oee = calculator.Calculate(run.Value, now, losses);

            foreach (var (key, value) in ScopeValues(oee, slot.Scope, request.PostId))
                values[key] = value;
        }

        foreach (var identifier in expression!.Identifiers)
        {
            if (request.Values is not null && request.Values.TryGetValue(identifier, out var manual) && request.RunId is null)
                values[identifier] = manual;
            else
                values.TryAdd(identifier, 0);
        }

        var result = expression.Evaluate(values);

        return Result.Ok(new OeeFormulaTestResponse(
            true,
            null,
            null,
            [.. expression.Identifiers.Order()],
            Math.Round(result, 6),
            expression.Identifiers.Order().ToDictionary(identifier => identifier, identifier => Math.Round(values[identifier], 6))));
    }

    private static OeeFormulaTestResponse Failed(string message, int position, IReadOnlyList<string>? identifiers = null) =>
        new(false, message, position, identifiers ?? [], null, new Dictionary<string, double>());

    private static IReadOnlyDictionary<string, double> ScopeValues(RunOee oee, OeeScope scope, ulong? postId)
    {
        if (scope is OeeScope.PostPillar or OeeScope.PostOee)
        {
            var post = (postId is { } id ? oee.Posts.FirstOrDefault(candidate => candidate.PostId == id) : null)
                ?? oee.Posts.Where(candidate => candidate.Measured).OrderBy(candidate => candidate.X).FirstOrDefault();

            if (post is null)
                return new Dictionary<string, double>();

            return new Dictionary<string, double>(post.Variables)
            {
                ["availability"] = post.Pillars.Availability,
                ["performance"] = post.Pillars.Performance,
                ["quality"] = post.Pillars.Quality,
            };
        }

        return new Dictionary<string, double>(oee.Line.Variables)
        {
            ["availability"] = oee.Line.Pillars.Availability,
            ["performance"] = oee.Line.Pillars.Performance,
            ["quality"] = oee.Line.Pillars.Quality,
            ["machine_oee"] = oee.Line.Variables.GetValueOrDefault(OeeCatalog.AggregateKey("machine_oee", "average")),
        };
    }
}
