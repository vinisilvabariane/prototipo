using IrisWeb.Domain.Enums;

namespace IrisWeb.Application.Communication.Argos.Requests;

public sealed record ArgosCyclesQuery
{
    public DateTime? From { get; init; }

    public DateTime? To { get; init; }

    public ArgosShiftFilter Shift { get; init; } = ArgosShiftFilter.All;

    public ArgosResultFilter Result { get; init; } = ArgosResultFilter.All;

    public string? Station { get; init; }

    public string? Setup { get; init; }

    public int? Page { get; init; }

    public int? PageSize { get; init; }
}
