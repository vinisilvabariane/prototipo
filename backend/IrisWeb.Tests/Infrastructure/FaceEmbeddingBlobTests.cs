using IrisWeb.Infrastructure.Mappings;

namespace IrisWeb.Tests.Infrastructure;

public sealed class FaceEmbeddingBlobTests
{
    [Fact]
    public void ToBytes_ProducesFourBytesPerFloat()
    {
        var bytes = FaceEmbeddingBlob.ToBytes([1f, 2f, 3f, 4f]);

        Assert.Equal(16, bytes.Length);
    }

    [Fact]
    public void ToBytes_IsLittleEndianIeee754()
    {

        var bytes = FaceEmbeddingBlob.ToBytes([1f]);

        Assert.Equal(new byte[] { 0x00, 0x00, 0x80, 0x3F }, bytes);
    }

    [Fact]
    public void RoundTrip_PreservesEveryValueExactly()
    {
        float[] original = [0f, -1.5f, 3.14159f, float.Epsilon, -0.000123f, 12345.678f];

        var restored = FaceEmbeddingBlob.ToFloats(FaceEmbeddingBlob.ToBytes(original));

        Assert.Equal(original, restored);
    }

    [Fact]
    public void RoundTrip_HandlesTheRealVectorLength()
    {
        var original = new float[512];
        for (var i = 0; i < original.Length; i++)
            original[i] = i / 512f;

        var restored = FaceEmbeddingBlob.ToFloats(FaceEmbeddingBlob.ToBytes(original));

        Assert.Equal(2048, FaceEmbeddingBlob.ToBytes(original).Length);
        Assert.Equal(original, restored);
    }

    [Fact]
    public void ToFloats_ReadsABufferWrittenByNumpy()
    {

        byte[] fromNumpy =
        [
            0x00, 0x00, 0x80, 0x3F,
            0x00, 0x00, 0x00, 0x3F,
            0x00, 0x00, 0x80, 0xBE,
        ];

        Assert.Equal([1.0f, 0.5f, -0.25f], FaceEmbeddingBlob.ToFloats(fromNumpy));
    }
}
