using MeOne.Domain.Enums;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;

namespace MeOne.Domain.Entities;

public sealed class Supplier : Entity<ulong>
{
    private readonly List<SupplierTool> _tools = [];

    private readonly List<SupplierMaterial> _materials = [];

    private readonly List<SupplierSelfAssessment> _selfAssessments = [];

    private readonly List<SupplierAudit> _audits = [];

    private readonly List<SupplierDocument> _documents = [];

    private Supplier() { }

    public string SapCode { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Branch { get; private set; } = string.Empty;

    public MultiBranch MultiBranch { get; private set; }

    public SupplierOrigin Origin { get; private set; }

    public string? TaxId { get; private set; }

    public string? CorporateName { get; private set; }

    public SupplierCriticality Criticality { get; private set; }

    public string ZipCode { get; private set; } = string.Empty;

    public string District { get; private set; } = string.Empty;

    public string? Street { get; private set; }

    public string? Number { get; private set; }

    public string? ContactName { get; private set; }

    public string? Phone { get; private set; }

    public string? Email { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public ulong CreatedBy { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public ulong UpdatedBy { get; private set; }

    public IReadOnlyList<SupplierTool> Tools => [.. _tools.Where(item => item.Active)];

    public IReadOnlyList<SupplierMaterial> Materials => [.. _materials.Where(item => item.Active)];

    public IReadOnlyList<SupplierSelfAssessment> SelfAssessments => [.. _selfAssessments.Where(item => item.Active)];

    public IReadOnlyList<SupplierAudit> Audits => [.. _audits.Where(item => item.Active)];

    public IReadOnlyList<SupplierDocument> Documents => _documents;

    public SupplierQualification? CurrentQualification
    {
        get
        {
            var audit = Audits.OrderByDescending(item => item.AuditedOn).ThenByDescending(item => item.Id).FirstOrDefault();

            if (audit is not null)
                return new(QualificationKind.OnSiteAudit, audit.Status, audit.Score, audit.AuditedOn, audit.ExpiresOn, AuditPending: false);

            var self = SelfAssessments.OrderByDescending(item => item.AssessedOn).ThenByDescending(item => item.Id).FirstOrDefault();

            return self is null
                ? null
                : new(QualificationKind.SelfAssessment, self.Status, self.Score, self.AssessedOn, self.ExpiresOn, self.NeedsOnSiteAudit == true);
        }
    }

    public static string NormalizeSapCode(string code) => code.Trim().ToUpperInvariant();

    public static Supplier Create(
        string sapCode,
        string name,
        string branch,
        MultiBranch multiBranch,
        SupplierOrigin origin,
        string? taxId,
        string? corporateName,
        SupplierCriticality criticality,
        string zipCode,
        string district,
        string? street,
        string? number,
        string? contactName,
        string? phone,
        string? email,
        ulong creatorId,
        DateTime now)
    {
        var supplier = new Supplier { CreatedAt = now, CreatedBy = creatorId, UpdatedAt = now, UpdatedBy = creatorId };

        supplier.Apply(sapCode, name, branch, multiBranch, origin, taxId, corporateName, criticality, zipCode, district, street, number, contactName, phone, email);
        return supplier;
    }

    public bool Update(
        string sapCode,
        string name,
        string branch,
        MultiBranch multiBranch,
        SupplierOrigin origin,
        string? taxId,
        string? corporateName,
        SupplierCriticality criticality,
        string zipCode,
        string district,
        string? street,
        string? number,
        string? contactName,
        string? phone,
        string? email,
        ulong userId,
        DateTime now)
    {
        var changed = Apply(sapCode, name, branch, multiBranch, origin, taxId, corporateName, criticality, zipCode, district, street, number, contactName, phone, email);

        if (changed)
            MarkUpdated(userId, now);

        return changed;
    }

    public void MarkUpdated(ulong userId, DateTime now)
    {
        UpdatedBy = userId;
        UpdatedAt = now;
    }

    public SupplierTool? FindTool(ulong id) => Tools.FirstOrDefault(item => item.Id == id);

    public SupplierMaterial? FindMaterial(ulong id) => Materials.FirstOrDefault(item => item.Id == id);

    public SupplierSelfAssessment? FindSelfAssessment(ulong id) => SelfAssessments.FirstOrDefault(item => item.Id == id);

    public SupplierAudit? FindAudit(ulong id) => Audits.FirstOrDefault(item => item.Id == id);

    public SupplierDocument? FindDocument(ulong id) => _documents.FirstOrDefault(item => item.Id == id);

    public SupplierTool AddTool(string name, string? assetTag)
    {
        var tool = SupplierTool.Create(name, assetTag);
        _tools.Add(tool);
        return tool;
    }

    public SupplierMaterial AddMaterial(
        string sku,
        string? businessUnit,
        string? materialType,
        SupplierCriticality? criticality,
        bool? hasSapSpecification)
    {
        var material = SupplierMaterial.Create(sku, businessUnit, materialType, criticality, hasSapSpecification);
        _materials.Add(material);
        return material;
    }

    public SupplierSelfAssessment AddSelfAssessment(
        ulong responsibleId,
        DateOnly assessedOn,
        decimal? score,
        QualificationStatus status,
        string? editableLink,
        string? documentCode,
        int? revision,
        bool? needsOnSiteAudit)
    {
        var assessment = SupplierSelfAssessment.Create(responsibleId, assessedOn, score, status, editableLink, documentCode, revision, needsOnSiteAudit);
        _selfAssessments.Add(assessment);
        return assessment;
    }

    public SupplierAudit AddAudit(
        ulong responsibleId,
        DateOnly auditedOn,
        int validityMonths,
        decimal? score,
        QualificationStatus status,
        string? checklistCode,
        int? checklistRevision)
    {
        var audit = SupplierAudit.Create(responsibleId, auditedOn, validityMonths, score, status, checklistCode, checklistRevision);
        _audits.Add(audit);
        return audit;
    }

    public void RemoveTool(SupplierTool tool) => tool.Remove();

    public void RemoveMaterial(SupplierMaterial material) => material.Remove();

    public void RemoveSelfAssessment(SupplierSelfAssessment assessment) => assessment.Remove();

    public void RemoveAudit(SupplierAudit audit) => audit.Remove();

    public string? CheckDocumentTarget(SupplierDocumentKind kind, ulong? ownerId)
    {
        if (!SupplierDocumentRules.BelongsToAnItem(kind))
            return ownerId is null ? null : SupplierErrors.DocumentOwnerNotAllowed;

        if (ownerId is not { } id)
            return SupplierErrors.DocumentOwnerRequired;

        return kind switch
        {
            SupplierDocumentKind.ToolDrawing => FindTool(id) is null ? SupplierErrors.ToolNotFound : null,
            SupplierDocumentKind.MaterialDocument => FindMaterial(id) is null ? SupplierErrors.MaterialNotFound : null,
            SupplierDocumentKind.AuditReport => FindAudit(id) is null ? SupplierErrors.AuditNotFound : null,
            _ => FindSelfAssessment(id) is null ? SupplierErrors.SelfAssessmentNotFound : null,
        };
    }

    public IReadOnlyList<SupplierDocument> AttachDocument(
        SupplierDocumentKind kind,
        ulong? ownerId,
        string originalName,
        string storedName,
        string contentType,
        long size,
        string? version,
        DateOnly? expiresOn,
        ulong userId,
        DateTime now)
    {
        if (CheckDocumentTarget(kind, ownerId) is { } problem)
            throw new DomainRuleException(problem);

        var replaced = SupplierDocumentRules.ReplacesPrevious(kind)
            ? _documents.Where(document => document.Kind == kind && document.OwnerId == ownerId).ToList()
            : [];

        _documents.RemoveAll(document => replaced.Contains(document));
        _documents.Add(SupplierDocument.Create(kind, ownerId, originalName, storedName, contentType, size, version, expiresOn, userId, now));

        return replaced;
    }

    public SupplierDocument? DetachDocument(ulong id)
    {
        var document = FindDocument(id);

        if (document is not null)
            _documents.Remove(document);

        return document;
    }

    public IReadOnlyList<SupplierDocument> DocumentsOf(SupplierDocumentKind kind, ulong? ownerId) =>
        [.. _documents.Where(document => document.Kind == kind && document.OwnerId == ownerId)];

    private bool Apply(
        string sapCode,
        string name,
        string branch,
        MultiBranch multiBranch,
        SupplierOrigin origin,
        string? taxId,
        string? corporateName,
        SupplierCriticality criticality,
        string zipCode,
        string district,
        string? street,
        string? number,
        string? contactName,
        string? phone,
        string? email)
    {
        var next = (
            SapCode: NormalizeSapCode(sapCode),
            Name: name.Trim(),
            Branch: branch.Trim(),
            MultiBranch: multiBranch,
            Origin: origin,
            TaxId: NormalizeTaxId(taxId, origin),
            CorporateName: Clean(corporateName),
            Criticality: criticality,
            ZipCode: zipCode.Trim(),
            District: district.Trim(),
            Street: Clean(street),
            Number: Clean(number),
            ContactName: Clean(contactName),
            Phone: Clean(phone),
            Email: string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant());

        var current = (SapCode, Name, Branch, MultiBranch, Origin, TaxId, CorporateName, Criticality, ZipCode, District, Street, Number, ContactName, Phone, Email);

        if (current == next)
            return false;

        (SapCode, Name, Branch, MultiBranch, Origin, TaxId, CorporateName, Criticality, ZipCode, District, Street, Number, ContactName, Phone, Email) = next;
        return true;
    }

    public static string? NormalizeTaxId(string? taxId, SupplierOrigin origin)
    {
        if (string.IsNullOrWhiteSpace(taxId))
            return null;

        return origin == SupplierOrigin.Domestic ? Cnpj.Digits(taxId) : taxId.Trim();
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
