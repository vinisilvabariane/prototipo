using System.Text.Json;

namespace IrisWeb.Application.Communication.ModelCatalog.Requests;

public sealed record CompleteUploadRequest(long Size, string Sha256, JsonElement Manifest);
