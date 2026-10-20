namespace MeOne.Application.Communication.Oee.Responses;

public sealed record OeeFormulasResponse(
    string Availability,
    string Performance,
    string Quality,
    string MachineOee,
    string LineAvailability,
    string LinePerformance,
    string LineQuality,
    string LineOee);
