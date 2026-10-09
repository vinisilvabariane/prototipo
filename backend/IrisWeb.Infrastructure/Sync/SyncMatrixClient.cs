using System.Net.Http.Json;
using System.Text.Json;
using IrisWeb.Application.Communication.Matrix.Dtos;
using IrisWeb.Application.Errors;
using IrisWeb.Application.Interfaces.Matrix;

namespace IrisWeb.Infrastructure.Sync;

public sealed class SyncMatrixClient(HttpClient http) : ISyncMatrixClient
{
    private const string ServiceName = "Sync";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<string>> ListLinesAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<List<string>>("production-line", cancellationToken) ?? [];

    public async Task<IReadOnlyList<SyncConfiguredProduct>> ListConfiguredProductsAsync(
        string line,
        CancellationToken cancellationToken = default) =>
        await GetAsync<List<SyncConfiguredProduct>>(
            $"production-line-layout/configured-products?line={Uri.EscapeDataString(line)}",
            cancellationToken) ?? [];

    public async Task<SyncLayout> GetLayoutAsync(
        string line,
        string product,
        string shift,
        CancellationToken cancellationToken = default)
    {
        var path = "production-line-layout/get-by-line-and-product" +
                   $"?line={Uri.EscapeDataString(line)}" +
                   $"&product={Uri.EscapeDataString(product)}" +
                   $"&shift={Uri.EscapeDataString(shift)}";

        var layout = await GetAsync<SyncLayout>(path, cancellationToken);

        if (layout is null)
        {
            const string vazio = "O Sync devolveu um layout vazio.";

            throw new SyncMatrixException(vazio, 502, vazio);
        }

        return layout;
    }

    public async Task<SyncCollaborator> GetCollaboratorByRegistrationAsync(
        int registration,
        CancellationToken cancellationToken = default)
    {
        var collaborator = await GetAsync<SyncCollaborator>(
            $"colaborator/get-by-registration?registration={registration}",
            cancellationToken);

        if (collaborator is null)
        {
            const string vazio = "O Sync devolveu um colaborador vazio.";

            throw new SyncMatrixException(vazio, 502, vazio);
        }

        return collaborator;
    }

    public async Task SetupCollaboratorAsync(SyncSetupCollaborator request, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync("training-matrix/setup-collaborator", request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task RemoveCollaboratorAsync(SyncRemoveCollaborator request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Delete, "training-matrix/remove-collaborator")
        {
            Content = JsonContent.Create(request, options: JsonOptions),
        };
        using var response = await http.SendAsync(message, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<SyncShift>> ListShiftsAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<List<SyncShift>>("shifts", cancellationToken) ?? [];

    public async Task<IReadOnlyList<SyncRecommendedCollaborator>> ListRecommendedAsync(
        int stationId,
        int layoutId,
        int activityId,
        string product,
        int shiftId,
        CancellationToken cancellationToken = default)
    {
        var path = "training-matrix/get-collaborators-recommended-by-acitivity" +
                   $"?postId={stationId}" +
                   $"&layoutId={layoutId}" +
                   $"&activityId={activityId}" +
                   $"&product={Uri.EscapeDataString(product)}" +
                   $"&shiftId={shiftId}";

        return await GetAsync<List<SyncRecommendedCollaborator>>(path, cancellationToken) ?? [];
    }

    public async Task<SyncCollaboratorInformation> GetCollaboratorForActivityAsync(
        int registration,
        int activityId,
        string product,
        string shift,
        int stationId,
        int layoutId,
        CancellationToken cancellationToken = default)
    {
        var path = "training-matrix/get-collaborator-by-registration" +
                   $"?registration={registration}" +
                   $"&activityId={activityId}" +
                   $"&product={Uri.EscapeDataString(product)}" +
                   $"&shift={Uri.EscapeDataString(shift)}" +
                   $"&postId={stationId}" +
                   $"&layoutId={layoutId}";

        var collaborator = await GetAsync<SyncCollaboratorInformation>(path, cancellationToken);

        if (collaborator is null)
        {
            const string vazio = "O Sync devolveu um colaborador vazio.";

            throw new SyncMatrixException(vazio, 502, vazio);
        }

        return collaborator;
    }

    private async Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await http.GetAsync(path, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var status = (int)response.StatusCode;

        throw new SyncMatrixException($"{ServiceName} respondeu {status}.", status, DetailFor(status, body));
    }

    private static string DetailFor(int statusCode, string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return $"O {ServiceName} respondeu {statusCode} sem corpo.";

        try
        {
            using var document = JsonDocument.Parse(body);

            if (document.RootElement.ValueKind == JsonValueKind.Object &&
                document.RootElement.TryGetProperty("details", out var details) &&
                details.ValueKind == JsonValueKind.String)
            {
                return details.GetString() ?? body;
            }
        }
        catch (JsonException)
        {
        }

        return body;
    }
}
