namespace MeOne.Application.Communication.Workflows.Responses;

public sealed record WorkflowStatsResponse(int Total, int InProgress, int Approved, int Rejected);
