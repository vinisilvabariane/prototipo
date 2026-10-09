namespace IrisWeb.Application.Communication.Matrix.Dtos;

public sealed record SyncRemoveCollaborator(int CollaboratorId, int LayoutId, int PostId);
