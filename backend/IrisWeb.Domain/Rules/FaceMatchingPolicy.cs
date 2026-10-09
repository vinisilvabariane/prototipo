namespace IrisWeb.Domain.Rules;

public sealed record FaceMatchingPolicy(
    string ModelVersion,
    int MatchTopK,
    float DistanceThreshold,
    float DistanceMargin)
{

    public static FaceMatchingPolicy Default { get; } = new("inception_resnet_v1_aligned", 2, 0.85f, 0.12f);

    public string ResolveModelVersion(string? requested) =>
        string.IsNullOrWhiteSpace(requested) ? ModelVersion : requested;

    public bool IsCoherent =>
        DistanceThreshold > 0
        && DistanceMargin >= 0
        && MatchTopK >= 1
        && !string.IsNullOrWhiteSpace(ModelVersion);
}
