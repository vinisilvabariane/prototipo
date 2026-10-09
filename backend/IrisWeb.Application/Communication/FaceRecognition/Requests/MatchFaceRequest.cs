namespace IrisWeb.Application.Communication.FaceRecognition.Requests;

public sealed record MatchFaceRequest(float[] Embedding, string ModelVersion);
