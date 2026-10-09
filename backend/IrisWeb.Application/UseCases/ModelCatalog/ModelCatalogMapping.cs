using System.Text.Json;
using IrisWeb.Application.Communication.ModelCatalog.Responses;
using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Enums;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Rules;
using IrisWeb.Application.Interfaces;

namespace IrisWeb.Application.UseCases.ModelCatalog;

internal static class ModelCatalogMapping
{

    public static string? Actor(ICurrentActor currentActor) =>
        string.IsNullOrWhiteSpace(currentActor.Registration) ? null : currentActor.Registration;

    public static string ArtifactUrl(Guid modelId, Guid versionId) =>
        $"/api/v1/models/{modelId}/versions/{versionId}/artifact";

    public static string InvalidTransition(ModelVersionStatus expected, ModelVersionStatus actual) =>
        ModelCatalogErrors.InvalidTransition(expected.ToString(), actual.ToString());

    public static string? RuntimeViolation(AiModel model, string runtime) =>
        ModelPlatformPolicy.AcceptsRuntime(model.Platform, runtime)
            ? null
            : ModelCatalogErrors.RuntimeMismatch(
                model.Platform.ToString(),
                ModelPlatformPolicy.RequiredRuntime(model.Platform),
                runtime.Trim());

    public static string? ManifestViolation(AiModel model, JsonElement manifest)
    {
        if (manifest.ValueKind != JsonValueKind.Object)
            return null;

        var platform = model.Platform;

        if (TextOf(manifest, "format") is { } format && !ModelPlatformPolicy.AcceptsFormat(platform, format))
        {
            return ModelCatalogErrors.ManifestFormatMismatch(
                platform.ToString(), ModelPlatformPolicy.RequiredFormat(platform), format);
        }

        if (TextOf(manifest, "entrypoint") is { } entrypoint &&
            !ModelPlatformPolicy.AcceptsEntrypoint(platform, entrypoint))
        {
            return ModelCatalogErrors.EntrypointMismatch(
                platform.ToString(), ModelPlatformPolicy.RequiredEntrypointExtension(platform), entrypoint);
        }

        return null;
    }

    private static string? TextOf(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    // O espaço de embedding é quem o front declara no manifesto (`manifest.embeddingSpace`) e manda
    // em `modelVersion` no match/enroll — não é o mesmo texto que `ModelVersion.Version` (o número da
    // versão do catálogo). Uma versão sem o campo, ou com manifesto ausente/inválido, não declara espaço.
    public static string? ResolveEmbeddingSpace(ModelVersion version)
    {
        var manifest = ParseManifest(version.ManifestJson);
        return manifest is { } value ? TextOf(value, "embeddingSpace") : null;
    }

    public static CatalogModelResponse ToResponse(AiModel model) => new(
        model.Id, model.Key, model.Name, model.Description, model.TaskType, model.Platform,
        model.CreatedBy, model.CreatedAtUtc,
        model.Versions.OrderByDescending(x => x.CreatedAtUtc).Select(ToResponse).ToList(),
        model.ActiveVersionId);

    public static CatalogModelVersionResponse ToResponse(ModelVersion version) => new(
        version.Id, version.Version, version.Runtime, version.Status, version.ArtifactSize, version.Sha256,
        ParseManifest(version.ManifestJson),
        version.CreatedBy, version.ApprovedBy, version.CreatedAtUtc, version.ApprovedAtUtc);

    private static JsonElement? ParseManifest(string? json)
    {
        if (json is null)
            return null;

        try
        {
            return JsonSerializer.Deserialize<JsonElement>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
