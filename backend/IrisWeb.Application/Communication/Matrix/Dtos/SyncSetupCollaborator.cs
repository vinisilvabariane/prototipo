namespace IrisWeb.Application.Communication.Matrix.Dtos;

public sealed record SyncSetupCollaborator(
    int CollaboratorId,
    int LayoutId,
    int PostId,
    int ActivityId,
    string Shift,
    string Product);
