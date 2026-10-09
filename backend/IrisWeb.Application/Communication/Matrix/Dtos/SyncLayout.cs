namespace IrisWeb.Application.Communication.Matrix.Dtos;

public sealed record SyncLayout(int Id, IReadOnlyList<SyncLayoutStation>? Configuration);
