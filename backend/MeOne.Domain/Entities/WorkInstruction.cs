namespace MeOne.Domain.Entities;

public sealed class WorkInstruction : Entity<ulong>
{
    private readonly List<WorkInstructionStep> _steps = [];

    private WorkInstruction() { }

    public string Code { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string DocumentNumber { get; private set; } = string.Empty;

    public string Product { get; private set; } = string.Empty;

    public string PreparedBy { get; private set; } = string.Empty;

    public string ApprovedBy { get; private set; } = string.Empty;

    public string RequiredPpe { get; private set; } = string.Empty;

    public string ProcessSpecifications { get; private set; } = string.Empty;

    public string? Notes { get; private set; }

    public string PdfStoredName { get; private set; } = string.Empty;

    public long PdfSize { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public ulong CreatedBy { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public ulong UpdatedBy { get; private set; }

    public IReadOnlyList<WorkInstructionStep> Steps => _steps;

    public static WorkInstruction Create(
        string code,
        string title,
        string documentNumber,
        string product,
        string preparedBy,
        string approvedBy,
        string requiredPpe,
        string processSpecifications,
        string? notes,
        string pdfStoredName,
        long pdfSize,
        ulong creatorId,
        DateTime now)
    {
        var instruction = new WorkInstruction
        {
            PdfStoredName = pdfStoredName,
            PdfSize = pdfSize,
            CreatedAt = now,
            CreatedBy = creatorId,
            UpdatedAt = now,
            UpdatedBy = creatorId,
        };

        instruction.Apply(code, title, documentNumber, product, preparedBy, approvedBy, requiredPpe, processSpecifications, notes);
        return instruction;
    }

    public bool Update(
        string code,
        string title,
        string documentNumber,
        string product,
        string preparedBy,
        string approvedBy,
        string requiredPpe,
        string processSpecifications,
        string? notes,
        ulong userId,
        DateTime now)
    {
        var changed = Apply(code, title, documentNumber, product, preparedBy, approvedBy, requiredPpe, processSpecifications, notes);

        if (changed)
            Touch(userId, now);

        return changed;
    }

    public string ReplacePdf(string storedName, long size, ulong userId, DateTime now)
    {
        var previous = PdfStoredName;
        PdfStoredName = storedName;
        PdfSize = size;
        Touch(userId, now);
        return previous;
    }

    public IReadOnlyList<WorkInstructionStep> SetSteps(IReadOnlyList<WorkInstructionStep> ordered, ulong userId, DateTime now)
    {
        var previous = _steps.ToList();
        var removed = previous.Where(step => !ordered.Contains(step)).ToList();
        var changed = removed.Count > 0 || !previous.SequenceEqual(ordered);

        _steps.Clear();
        _steps.AddRange(ordered);

        for (var index = 0; index < _steps.Count; index++)
            _steps[index].Place(index + 1);

        if (changed)
            Touch(userId, now);

        return removed;
    }

    public void MarkEdited(ulong userId, DateTime now) => Touch(userId, now);

    public static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();

    private bool Apply(
        string code,
        string title,
        string documentNumber,
        string product,
        string preparedBy,
        string approvedBy,
        string requiredPpe,
        string processSpecifications,
        string? notes)
    {
        var next = (
            Code: NormalizeCode(code),
            Title: title.Trim(),
            DocumentNumber: documentNumber.Trim(),
            Product: product.Trim(),
            PreparedBy: preparedBy.Trim(),
            ApprovedBy: approvedBy.Trim(),
            RequiredPpe: requiredPpe.Trim(),
            ProcessSpecifications: processSpecifications.Trim(),
            Notes: string.IsNullOrWhiteSpace(notes) ? null : notes.Trim());

        var current = (Code, Title, DocumentNumber, Product, PreparedBy, ApprovedBy, RequiredPpe, ProcessSpecifications, Notes);

        if (current == next)
            return false;

        (Code, Title, DocumentNumber, Product, PreparedBy, ApprovedBy, RequiredPpe, ProcessSpecifications, Notes) = next;
        return true;
    }

    private void Touch(ulong userId, DateTime now)
    {
        UpdatedBy = userId;
        UpdatedAt = now;
    }
}
