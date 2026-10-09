namespace IrisWeb.Application.Communication.ModelCatalog.Responses;

public sealed record UploadUrlResponse(Guid VersionId, string Method, string Url, DateTime ExpiresAtUtc);
