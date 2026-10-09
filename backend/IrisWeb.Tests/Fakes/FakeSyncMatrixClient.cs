using IrisWeb.Application.Communication.Matrix.Dtos;
using IrisWeb.Application.Errors;
using IrisWeb.Application.Interfaces.Matrix;

namespace IrisWeb.Tests.Fakes;

public sealed class FakeSyncMatrixClient : ISyncMatrixClient
{
    public List<string> Lines { get; } = [];

    public List<SyncConfiguredProduct> Products { get; } = [];

    public SyncLayout Layout { get; set; } = new(1, []);

    public Dictionary<int, SyncCollaborator> Collaborators { get; } = [];

    public List<SyncSetupCollaborator> Setups { get; } = [];

    public Exception? Failure { get; set; }

    public Exception? SetupFailure { get; set; }

    public List<SyncRemoveCollaborator> Removals { get; } = [];

    public List<SyncShift> Shifts { get; } = [new(1, "1º TURNO"), new(2, "2º TURNO")];

    public List<SyncRecommendedCollaborator> Recommended { get; } = [];

    public Dictionary<int, SyncCollaboratorInformation> ActivityCollaborators { get; } = [];

    public (int StationId, int LayoutId, int ActivityId, string Product, int ShiftId)? LastRecommendationQuery { get; private set; }

    public (int Registration, int ActivityId, string Product, string Shift, int StationId, int LayoutId)? LastCandidateQuery { get; private set; }

    public (string Line, string Product, string Shift)? LastLayoutQuery { get; private set; }

    public Task<IReadOnlyList<string>> ListLinesAsync(CancellationToken cancellationToken = default) =>
        Respond<IReadOnlyList<string>>(Lines);

    public Task<IReadOnlyList<SyncConfiguredProduct>> ListConfiguredProductsAsync(
        string line,
        CancellationToken cancellationToken = default) =>
        Respond<IReadOnlyList<SyncConfiguredProduct>>(Products);

    public Task<SyncLayout> GetLayoutAsync(
        string line,
        string product,
        string shift,
        CancellationToken cancellationToken = default)
    {
        LastLayoutQuery = (line, product, shift);
        return Respond(Layout);
    }

    public Task<SyncCollaborator> GetCollaboratorByRegistrationAsync(
        int registration,
        CancellationToken cancellationToken = default)
    {
        if (Collaborators.TryGetValue(registration, out var collaborator))
            return Respond(collaborator);

        return Task.FromException<SyncCollaborator>(
            new SyncMatrixException("Sync respondeu 400.", 400, "Matricula não encontrada"));
    }

    public Task SetupCollaboratorAsync(SyncSetupCollaborator request, CancellationToken cancellationToken = default)
    {
        if (SetupFailure is not null)
            return Task.FromException(SetupFailure);

        Setups.Add(request);
        return Task.CompletedTask;
    }

    public Task RemoveCollaboratorAsync(SyncRemoveCollaborator request, CancellationToken cancellationToken = default)
    {
        if (SetupFailure is not null)
            return Task.FromException(SetupFailure);

        Removals.Add(request);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<SyncShift>> ListShiftsAsync(CancellationToken cancellationToken = default) =>
        Respond<IReadOnlyList<SyncShift>>(Shifts);

    public Task<IReadOnlyList<SyncRecommendedCollaborator>> ListRecommendedAsync(
        int stationId,
        int layoutId,
        int activityId,
        string product,
        int shiftId,
        CancellationToken cancellationToken = default)
    {
        LastRecommendationQuery = (stationId, layoutId, activityId, product, shiftId);
        return Respond<IReadOnlyList<SyncRecommendedCollaborator>>(Recommended);
    }

    public Task<SyncCollaboratorInformation> GetCollaboratorForActivityAsync(
        int registration,
        int activityId,
        string product,
        string shift,
        int stationId,
        int layoutId,
        CancellationToken cancellationToken = default)
    {
        LastCandidateQuery = (registration, activityId, product, shift, stationId, layoutId);

        if (ActivityCollaborators.TryGetValue(registration, out var collaborator))
            return Respond(collaborator);

        return Task.FromException<SyncCollaboratorInformation>(
            new SyncMatrixException("Sync respondeu 400.", 400, "Colaborador não encontrado"));
    }

    public static SyncLayoutStation Station(
        int id,
        string name,
        int? registration = null,
        string? level = null,
        int? activityId = 429) =>
        new(id, name, false, registration is null ? null : id * 100, registration is null ? null : "FULANO", registration, level, activityId);

    private Task<T> Respond<T>(T value) =>
        Failure is null ? Task.FromResult(value) : Task.FromException<T>(Failure);
}
