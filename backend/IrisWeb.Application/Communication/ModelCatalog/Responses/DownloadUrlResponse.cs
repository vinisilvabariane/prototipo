namespace IrisWeb.Application.Communication.ModelCatalog.Responses;

public sealed record DownloadUrlResponse(Guid VersionId, string Url, DateTime ExpiresAtUtc);
