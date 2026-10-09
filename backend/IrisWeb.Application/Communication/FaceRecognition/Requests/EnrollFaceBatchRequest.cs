namespace IrisWeb.Application.Communication.FaceRecognition.Requests;

public sealed record EnrollFaceBatchRequest(
    string Registration,
    float[][] Embeddings,
    string ModelVersion,
    bool ConfirmSimilar = false);
