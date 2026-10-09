using IrisWeb.Domain.Enums;

namespace IrisWeb.Domain.Interfaces;

public sealed record ArgosCycleFilter(
    DateTime From,
    DateTime To,
    ArgosShiftFilter Shift,
    ArgosResultFilter Result,
    string? Station,
    string? Setup);
