namespace IrisWeb.Domain.Entities;

/// <summary>
/// Uma peca verificada pelo Route Verify. A tabela e escrita pelo iris-desktop; aqui so se le.
/// </summary>
public sealed class ArgosCycle : Entity<long>
{
    private ArgosCycle() { }

    public int Status { get; private set; }

    public decimal CycleSeconds { get; private set; }

    public string? Station { get; private set; }

    public int? SetupId { get; private set; }

    public string? SetupName { get; private set; }

    public string? FailReason { get; private set; }

    public string? FailedPoint { get; private set; }

    public float? MinConfidence { get; private set; }

    public DateTime RecordedAt { get; private set; }

    public bool IsOk => Status == 1;

    public static ArgosCycle Restore(
        long id,
        int status,
        decimal cycleSeconds,
        DateTime recordedAt,
        string? station = null,
        string? setupName = null,
        string? failReason = null,
        string? failedPoint = null,
        float? minConfidence = null,
        int? setupId = null) =>
        new()
        {
            Id = id,
            Status = status,
            CycleSeconds = cycleSeconds,
            RecordedAt = recordedAt,
            Station = station,
            SetupName = setupName,
            FailReason = failReason,
            FailedPoint = failedPoint,
            MinConfidence = minConfidence,
            SetupId = setupId,
        };
}
