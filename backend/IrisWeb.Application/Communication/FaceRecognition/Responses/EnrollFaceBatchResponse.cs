namespace IrisWeb.Application.Communication.FaceRecognition.Responses;

/// <summary>
/// Outcome: enrolled (gravou), similar (parecido com outra matricula, precisa de confirmacao),
/// same_person (o rosto ja e de outra matricula, bloqueado), inconsistent (as fotos nao batem
/// entre si, refazer) ou registration_mismatch (a matricula ja tem cadastro com outro rosto).
/// So "enrolled" grava alguma coisa.
/// </summary>
public sealed record EnrollFaceBatchResponse(
    string Outcome,
    int Saved,
    string? SimilarRegistration,
    float? Distance,
    float Spread);
