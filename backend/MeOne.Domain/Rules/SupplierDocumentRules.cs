using MeOne.Domain.Enums;

namespace MeOne.Domain.Rules;

public static class SupplierDocumentRules
{
    public static bool ReplacesPrevious(SupplierDocumentKind kind) => kind is
        SupplierDocumentKind.Presentation or
        SupplierDocumentKind.SelfAssessmentAttachment or
        SupplierDocumentKind.Declaration or
        SupplierDocumentKind.AuditReport;

    public static bool BelongsToAnItem(SupplierDocumentKind kind) => kind is
        SupplierDocumentKind.ToolDrawing or
        SupplierDocumentKind.MaterialDocument or
        SupplierDocumentKind.SelfAssessmentAttachment or
        SupplierDocumentKind.Declaration or
        SupplierDocumentKind.AuditReport;

    public static string? ImageOrPdfContentType(ReadOnlySpan<byte> header)
    {
        if (FileSignatures.IsPdf(header))
            return "application/pdf";

        var image = FileSignatures.ImageContentTypeOf(header);

        return image is "image/png" or "image/jpeg" ? image : null;
    }

    public static string ExtensionFor(string contentType) => contentType switch
    {
        "application/pdf" => "pdf",
        "image/png" => "png",
        "image/jpeg" => "jpg",
        _ => string.Empty,
    };
}
