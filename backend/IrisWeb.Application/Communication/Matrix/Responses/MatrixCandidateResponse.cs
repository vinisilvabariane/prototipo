namespace IrisWeb.Application.Communication.Matrix.Responses;

/// <summary>
/// Operador que pode ir para o posto, como o Sync mostra na alocacao: nivel na atividade do
/// posto, score e, quando veio da recomendacao, a prioridade e se ja tem treinamento valido
/// no posto+SKU. HasFaceEnrollment diz se a matricula tem cadastro facial no Iris; sem ele a
/// alocacao e recusada.
/// </summary>
public sealed record MatrixCandidateResponse(
    int Registration,
    string? Name,
    string? Level,
    string? Function,
    int Score,
    bool HasFaceEnrollment,
    int? PriorityGroup = null,
    bool? HasValidPostTraining = null,
    int? AllocatedPostsCount = null,
    int? RemainingSlots = null);
