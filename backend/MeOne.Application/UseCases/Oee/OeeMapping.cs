using MeOne.Application.Communication.Oee.Responses;
using MeOne.Domain.Entities;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Oee;

internal static class OeeMapping
{
    public const string DefaultSetName = "Padrão";

    public static double Percent(double ratio) => Math.Round(ratio * 100, 2, MidpointRounding.AwayFromZero);

    public static OeePillarsResponse ToResponse(PillarValues pillars) =>
        new(Percent(pillars.Availability), Percent(pillars.Performance), Percent(pillars.Quality), Percent(pillars.Oee));

    public static OeeHourResponse ToResponse(HourOee hour) =>
        new(hour.HourStart, (int)Math.Round(hour.Seconds), ToResponse(hour.Pillars), Math.Round(hour.Produced, 2), Math.Round(hour.Theoretical, 2));

    public static OeePostResponse ToResponse(PostOee post) => new(
        post.PostId,
        post.Name,
        post.Kind,
        post.X,
        post.TargetPerHour,
        post.Measured,
        ToResponse(post.Pillars),
        Math.Round(post.Variables.GetValueOrDefault("real_production"), 2),
        Math.Round(post.Variables.GetValueOrDefault("theoretical_production"), 2),
        Math.Round(post.Variables.GetValueOrDefault("occupied_seconds")),
        Math.Round(post.Variables.GetValueOrDefault("total_downtime")),
        post.Variables.GetValueOrDefault("defects"),
        [.. post.Hours.Select(ToResponse)]);

    public static RunOeeResponse ToResponse(
        ProductionRun run,
        ProductionLine line,
        ProductionProduct product,
        ProductionShift shift,
        RunOee oee,
        string formulaSetName) => new(
        run.Id,
        run.Status,
        line.Id,
        line.Name,
        product.Code,
        product.Name,
        shift.Name,
        oee.From,
        oee.To is { } to && run.EndedAt is not null ? to : null,
        (int)Math.Round(oee.ElapsedSeconds),
        ToResponse(oee.Line.Pillars),
        Math.Round(oee.Line.Variables.GetValueOrDefault("line_produced_station"), 2),
        Math.Round(oee.Line.Variables.GetValueOrDefault("line_plan"), 2),
        oee.Line.LastPostId,
        [.. oee.Posts.Select(ToResponse)],
        [.. oee.Line.Hours.Select(ToResponse)],
        oee.LossSourceAvailable,
        formulaSetName);

    public static OeeRunSummaryResponse ToSummary(
        ProductionRun run,
        ProductionLine line,
        ProductionProduct product,
        ProductionShift shift,
        RunOee oee) => new(
        run.Id,
        run.Status,
        line.Id,
        line.Name,
        product.Code,
        product.Name,
        shift.Name,
        oee.From,
        run.EndedAt,
        (int)Math.Round(oee.ElapsedSeconds),
        ToResponse(oee.Line.Pillars),
        Math.Round(oee.Line.Variables.GetValueOrDefault("line_produced_station"), 2),
        Math.Round(oee.Line.Variables.GetValueOrDefault("line_plan"), 2),
        oee.Posts.Count(post => post.Measured),
        oee.Posts.Count);

    public static OeeFormulasResponse ToResponse(OeeFormulas formulas) => new(
        formulas.Availability,
        formulas.Performance,
        formulas.Quality,
        formulas.MachineOee,
        formulas.LineAvailability,
        formulas.LinePerformance,
        formulas.LineQuality,
        formulas.LineOee);

    public static OeeFormulaSetResponse ToResponse(OeeFormulaSet set, IReadOnlyDictionary<ulong, User> users) => new(
        set.Id,
        set.Name,
        set.IsActive,
        ToResponse(set.Formulas),
        set.UpdatedAt,
        users.TryGetValue(set.UpdatedBy, out var user) ? user.Name : null);

    public static OeeFormulas ToDomain(Communication.Oee.Requests.OeeFormulasRequest request) => new(
        request.Availability ?? string.Empty,
        request.Performance ?? string.Empty,
        request.Quality ?? string.Empty,
        request.MachineOee ?? string.Empty,
        request.LineAvailability ?? string.Empty,
        request.LinePerformance ?? string.Empty,
        request.LineQuality ?? string.Empty,
        request.LineOee ?? string.Empty);

    public static string SlotGroup(OeeScope scope) => scope switch
    {
        OeeScope.PostPillar or OeeScope.PostOee => "post",
        _ => "line",
    };

    public static IReadOnlyList<string> SlotsOf(OeeVariable variable)
    {
        var slots = new List<string>();

        foreach (var (key, _, scope) in OeeFormulas.Slots)
        {
            if (variable.Scopes.Contains(scope))
                slots.Add(key);
        }

        return slots;
    }
}
