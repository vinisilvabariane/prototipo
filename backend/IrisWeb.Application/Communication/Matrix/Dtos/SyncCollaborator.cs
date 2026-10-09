namespace IrisWeb.Application.Communication.Matrix.Dtos;

public sealed record SyncCollaborator(
    int Id,
    int Registration,
    string? FullName,
    string? Function = null,
    string? BranchOffice = null);
