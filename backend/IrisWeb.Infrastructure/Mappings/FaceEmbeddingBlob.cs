namespace IrisWeb.Infrastructure.Mappings;

public static class FaceEmbeddingBlob
{
    public static byte[] ToBytes(float[] values)
    {
        var bytes = new byte[values.Length * sizeof(float)];
        Buffer.BlockCopy(values, 0, bytes, 0, bytes.Length);
        return bytes;
    }

    public static float[] ToFloats(byte[] blob)
    {
        var values = new float[blob.Length / sizeof(float)];
        Buffer.BlockCopy(blob, 0, values, 0, values.Length * sizeof(float));
        return values;
    }
}
