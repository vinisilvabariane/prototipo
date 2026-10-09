namespace IrisWeb.Domain.Rules;

public sealed record EnrollmentScreeningPolicy(
    float SamePersonDistance,
    float SimilarDistance,
    float MaxSpread)
{

    public static EnrollmentScreeningPolicy Default { get; } = new(0.5f, 1.0f, 0.7f);

    public bool IsCoherent =>
        SamePersonDistance > 0
        && SimilarDistance >= SamePersonDistance
        && MaxSpread > 0;
}
