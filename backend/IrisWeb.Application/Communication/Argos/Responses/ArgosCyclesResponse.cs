namespace IrisWeb.Application.Communication.Argos.Responses;

public sealed record ArgosCycleResponse(
    long Id,
    DateTime At,
    string Status,
    double CycleSeconds,
    string? Station,
    string? SetupName,
    string? FailReason,
    string? FailedPoint,
    double? MinConfidence);

public sealed record ArgosCyclesResponse(
    IReadOnlyList<ArgosCycleResponse> Entries,
    int Total,
    int Page,
    int PageSize);
