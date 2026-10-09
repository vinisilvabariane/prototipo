using IrisWeb.Domain.Entities;
using System.Numerics.Tensors;

namespace IrisWeb.Domain.Rules;

public enum EnrollmentVerdictKind
{
    Clear,
    SamePerson,
    Similar,
    Inconsistent,
    RegistrationMismatch,
}

public sealed record EnrollmentVerdict(
    EnrollmentVerdictKind Kind,
    string? ClosestRegistration,
    float? ClosestDistance,
    float Spread);

/// <summary>
/// Confere as fotos novas de um cadastro antes de gravar: se batem entre si, se batem com o
/// cadastro que a matricula ja tem e se nao ficam perto demais do rosto de outra pessoa.
/// A distancia para outra matricula segue a regra do match (media das K amostras mais proximas)
/// e fica com a pior foto do lote, porque e quadro a quadro que o reconhecimento confunde.
/// </summary>
public static class EnrollmentScreening
{
    public static EnrollmentVerdict Evaluate(
        string registration,
        IReadOnlyList<float[]> batch,
        IEnumerable<FaceEmbedding> enrolled,
        FaceMatchingPolicy matching,
        EnrollmentScreeningPolicy screening)
    {
        var clean = registration.Trim();
        var spread = Spread(batch);
        var topK = Math.Max(1, matching.MatchTopK);

        var byRegistration = enrolled
            .Where(embedding => batch.Count > 0 && embedding.IsComparableTo(batch[0]))
            .GroupBy(embedding => embedding.Registration)
            .ToList();

        var closest = byRegistration
            .Where(group => group.Key != clean)
            .Select(group => (Registration: group.Key, Distance: batch.Min(sample => Score(group, sample, topK))))
            .OrderBy(item => item.Distance)
            .Select(item => ((string, float)?)(item.Registration, item.Distance))
            .FirstOrDefault();

        var closestRegistration = closest?.Item1;
        var closestDistance = closest?.Item2;

        if (spread > screening.MaxSpread)
            return new EnrollmentVerdict(EnrollmentVerdictKind.Inconsistent, closestRegistration, closestDistance, spread);

        if (closestDistance < screening.SamePersonDistance)
            return new EnrollmentVerdict(EnrollmentVerdictKind.SamePerson, closestRegistration, closestDistance, spread);

        var own = byRegistration.FirstOrDefault(group => group.Key == clean);
        if (own is not null)
        {
            var ownDistance = batch.Average(sample => Score(own, sample, topK));
            if (ownDistance > matching.DistanceThreshold)
                return new EnrollmentVerdict(EnrollmentVerdictKind.RegistrationMismatch, clean, ownDistance, spread);
        }

        if (closestDistance < screening.SimilarDistance)
            return new EnrollmentVerdict(EnrollmentVerdictKind.Similar, closestRegistration, closestDistance, spread);

        return new EnrollmentVerdict(EnrollmentVerdictKind.Clear, closestRegistration, closestDistance, spread);
    }

    private static float Score(IEnumerable<FaceEmbedding> samples, float[] probe, int topK) =>
        samples
            .Select(embedding => embedding.DistanceTo(probe))
            .OrderBy(distance => distance)
            .Take(topK)
            .Average();

    private static float Spread(IReadOnlyList<float[]> batch)
    {
        var widest = 0f;
        for (var i = 0; i < batch.Count; i++)
            for (var j = i + 1; j < batch.Count; j++)
                widest = Math.Max(widest, TensorPrimitives.Distance(batch[i], batch[j]));
        return widest;
    }
}
