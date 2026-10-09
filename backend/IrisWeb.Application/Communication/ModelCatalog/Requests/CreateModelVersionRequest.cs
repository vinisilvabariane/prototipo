namespace IrisWeb.Application.Communication.ModelCatalog.Requests;

public sealed record CreateModelVersionRequest(string Version, string Runtime);
