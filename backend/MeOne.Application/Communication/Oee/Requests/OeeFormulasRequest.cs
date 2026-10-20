namespace MeOne.Application.Communication.Oee.Requests;

public sealed record OeeFormulasRequest(
    string? Availability,
    string? Performance,
    string? Quality,
    string? MachineOee,
    string? LineAvailability,
    string? LinePerformance,
    string? LineQuality,
    string? LineOee);
