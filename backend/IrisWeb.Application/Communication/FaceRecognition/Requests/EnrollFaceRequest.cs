namespace IrisWeb.Application.Communication.FaceRecognition.Requests;

public sealed record EnrollFaceRequest(string Registration, float[] Embedding, string ModelVersion);
