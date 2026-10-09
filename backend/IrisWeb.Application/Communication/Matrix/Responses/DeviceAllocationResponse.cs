namespace IrisWeb.Application.Communication.Matrix.Responses;

public sealed record DeviceAllocationResponse(
    DeviceMatrixResponse Binding,
    MatrixCollaboratorResponse? Allocated,
    DateTime CheckedAt);
