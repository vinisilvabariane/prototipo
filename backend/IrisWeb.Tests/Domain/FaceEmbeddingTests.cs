using IrisWeb.Domain.Entities;

namespace IrisWeb.Tests.Domain;

public sealed class FaceEmbeddingTests
{
    [Fact]
    public void Create_TrimsRegistrationAndDerivesDimension()
    {
        var embedding = FaceEmbedding.Create("  12345 ", [1f, 0f, 0f], "v1");

        Assert.Equal("12345", embedding.Registration);
        Assert.Equal(3, embedding.Dimension);
    }

    [Fact]
    public void DistanceTo_IsZeroForTheSameVector()
    {
        var embedding = FaceEmbedding.Create("12345", [1f, 0f, 0f], "v1");

        Assert.Equal(0f, embedding.DistanceTo([1f, 0f, 0f]), precision: 5);
    }

    [Fact]
    public void IsComparableTo_RequiresTheSameLength()
    {
        var embedding = FaceEmbedding.Create("12345", [1f, 0f, 0f], "v1");

        Assert.True(embedding.IsComparableTo([0f, 0f, 0f]));
        Assert.False(embedding.IsComparableTo([0f, 0f]));
    }
}
