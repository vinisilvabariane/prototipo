namespace IrisWeb.Domain.Entities;

public sealed class FaceEmbedding : Entity<long>
{
    private FaceEmbedding() { }

    private FaceEmbedding(string registration, float[] values, string modelVersion)
    {
        Registration = registration;
        Values = values;
        Dimension = values.Length;
        ModelVersion = modelVersion;
        CreatedAt = DateTime.Now;
    }

    public string Registration { get; private set; } = string.Empty;

    public float[] Values { get; private set; } = [];

    public int Dimension { get; private set; }

    public string ModelVersion { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    public static FaceEmbedding Create(string registration, float[] values, string modelVersion) =>
        new(registration.Trim(), values, modelVersion);

    public float DistanceTo(float[] other) =>
        System.Numerics.Tensors.TensorPrimitives.Distance(Values, other);

    public bool IsComparableTo(float[] other) => Values.Length == other.Length;
}
