namespace IrisWeb.Application.Communication.Collaborators.Responses;

public sealed record CollaboratorResponse(uint Registration, string FullName, string? Function, string? BranchOffice);
