namespace IrisWeb.Application.Communication.Argos.Responses;

public sealed record ArgosCountResponse(int Ok, int Ng);

public sealed record ArgosTotalsResponse(int Ok, int Ng, double? AvgCycleSeconds);

public sealed record ArgosByShiftResponse(ArgosCountResponse Adm, ArgosCountResponse T2);

public sealed record ArgosBucketResponse(DateTime Start, int Ok, int Ng, double? AvgCycleSeconds);

public sealed record ArgosFailReasonResponse(string? Reason, int Count);

public sealed record ArgosFailedPointResponse(string Point, int Count);

public sealed record ArgosStationResponse(
    string Station,
    int Ok,
    int Ng,
    double? AvgCycleSeconds,
    DateTime? LastCycleAt);

public sealed record ArgosSummaryResponse(
    DateTime From,
    DateTime To,
    string Bucket,
    DateTime GeneratedAt,
    DateTime? LastCycleAt,
    ArgosTotalsResponse Totals,
    ArgosByShiftResponse ByShift,
    IReadOnlyList<ArgosBucketResponse> Buckets,
    IReadOnlyList<ArgosFailReasonResponse> FailReasons,
    IReadOnlyList<ArgosFailedPointResponse> FailedPoints,
    IReadOnlyList<ArgosStationResponse> Stations);
