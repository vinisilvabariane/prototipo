namespace MeOne.Application.Communication.Workflows.Responses;

public sealed record WorkflowPermissionsResponse(
    bool IsCreator,
    bool IsAssignee,
    bool CanApprove,
    bool CanReject,
    bool CanEdit,
    bool CanManageAssignees);
