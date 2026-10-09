namespace IrisWeb.Domain.Entities;

/// <summary>
/// Registro de que alguem confirmou um cadastro parecido com o rosto de outra matricula
/// ("sao pessoas diferentes"). Serve de auditoria: quem confirmou, quando e o quanto parecia.
/// </summary>
public sealed class EnrollmentConfirmation : Entity<long>
{
    private EnrollmentConfirmation() { }

    private EnrollmentConfirmation(string registration, string similarRegistration, float distance, string confirmedBy)
    {
        Registration = registration;
        SimilarRegistration = similarRegistration;
        Distance = distance;
        ConfirmedBy = confirmedBy;
        ConfirmedAt = DateTime.Now;
    }

    public string Registration { get; private set; } = string.Empty;

    public string SimilarRegistration { get; private set; } = string.Empty;

    public float Distance { get; private set; }

    public string ConfirmedBy { get; private set; } = string.Empty;

    public DateTime ConfirmedAt { get; private set; }

    public static EnrollmentConfirmation Create(
        string registration,
        string similarRegistration,
        float distance,
        string confirmedBy) =>
        new(registration.Trim(), similarRegistration.Trim(), distance, confirmedBy.Trim());
}
